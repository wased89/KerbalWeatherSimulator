/*! \file AxisRenderer.h
    \brief Helper class to display axis lines.
    
    This class creates the world axis render lines.
*/
using System;
using UnityEngine;

public class AxisRenderer
{
    private GameObject lineX;
    private GameObject lineY;
    private GameObject lineZ;

    public AxisRenderer()
    {
        lineX = AxisFactory(new Vector3(2, 0, 0), Color.red, "X Axis");
        lineY = AxisFactory(new Vector3(0, 2, 0), Color.green, "Y Axis");
        lineZ = AxisFactory(new Vector3(0, 0, 2), Color.blue, "Z Axis");
    }

    private GameObject AxisFactory(Vector3 position, Color color, string name)
    {
        GameObject axis = new GameObject(name);
        

        // Create texture
        Texture2D newTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        newTex.SetPixel(0, 0, color);
        newTex.Apply();

        LineRenderer lr = axis.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Unlit/Texture"));
        lr.material.mainTexture = newTex;

        lr.SetWidth(0.1f, 0.1f);
        lr.SetVertexCount(2);
        lr.SetPosition(0, new Vector3(0,0,0));
        lr.SetPosition(1, position);

        return axis;
    }
}
