using System;
using UnityEngine;

public class AxisRenderer
{
    private Material lineMaterial;
    private GameObject lineX;
    private GameObject lineY;
    private GameObject lineZ;

    public AxisRenderer()
    {

        lineX = AxisFactory(new Vector3(2, 0, 0), Color.red);
        lineY = AxisFactory(new Vector3(0, 2, 0), Color.green);
        lineZ = AxisFactory(new Vector3(0, 0, 2), Color.blue);
        lineX.name = "X Axis";
        lineY.name = "Y Axis";
        lineZ.name = "Z Axis";
    }

    private GameObject AxisFactory(Vector3 position, Color color)
    {
        //GameObject returnObject = new GameObject();
        GameObject go = new GameObject();
        
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Unlit/Texture"));
        Texture2D newTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        newTex.SetPixel(0, 0, color);
        newTex.Apply();
        lr.material.mainTexture = newTex;
        lr.SetWidth(0.1f, 0.1f);
        lr.SetVertexCount(2);
        lr.SetPosition(0, new Vector3(0,0,0));
        lr.SetPosition(1, position);
        return go;
    }
}
