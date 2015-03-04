using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GeodesicGrid;
using KerbalWeatherSimulator;

namespace GUIUtils
{
    class WindTestShit
    {
        public Vector3 currentPos;
        private LineRenderer lineRend;
        private PlanetSimulator pSim;

        public WindTestShit(PlanetSimulator pSim, Vector3 startPos)
        {
            currentPos = startPos;
            this.pSim = pSim;
            
            //GameObject returnObject = new GameObject();
            GameObject go = new GameObject();

            lineRend = go.AddComponent<LineRenderer>();
            lineRend.material = new Material(Shader.Find("Unlit/Texture"));
            Texture2D newTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            newTex.SetPixel(0, 0, Color.white);
            newTex.Apply();
            lineRend.material.mainTexture = newTex;
            lineRend.SetWidth(0.01f, 0.01f);
            lineRend.SetVertexCount(2);
            lineRend.SetPosition(0, new Vector3(0, 0, 0));
            SetPosition(currentPos);
            
        }

        public void SetPosition(Vector3 pos)
        {
            lineRend.SetPosition(1, currentPos * 1.1f);
        }
        
        public void Update()
        {
            //600km radius = kerbin
            
            
            
            Cell rayCell = Cell.Containing(currentPos, pSim.level);

            Vector3 winDir = pSim.LiveMap[0][rayCell].WindDirection;
            currentPos += winDir * 0.0001f * Time.deltaTime;
            currentPos.Normalize();
            SetPosition(currentPos);
            
        }

    }
}
