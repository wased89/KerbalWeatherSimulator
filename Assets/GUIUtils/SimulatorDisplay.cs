using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Weather;
using GeodesicGrid;

namespace GUIUtils
{
    public class SimulatorDisplay
    {
        PlanetSimulator pSim;
        GameObject GO;
        DisplayMapType mapType;
        Texture2D displayTex;
        Mesh displayMesh;
        SunMove sunMove;
        

        public SimulatorDisplay(PlanetSimulator pSim, DisplayMapType dMapType)
        {
            this.pSim = pSim;
            this.mapType = dMapType;
            InitTexture();
            InitObject();
            

        }

        private void InitObject()
        {

            GO = new GameObject();
            GO.name = "Earth";
            MeshFilter MF = GO.AddComponent<MeshFilter>();
            MeshRenderer MR = GO.AddComponent<MeshRenderer>();
            MR.renderer.material.mainTexture = displayTex;

            displayMesh = new Mesh();
            displayMesh.vertices = Cell.AtLevel(pSim.level).Select(c => c.Position).ToArray();
            displayMesh.triangles = Triangle.AtLevel(pSim.level).SelectMany(t => t.GetVertices(pSim.level)).Select(c => (int)c.Index).ToArray();
            Vector2[] UV = new Vector2[displayMesh.vertexCount];
            for (int i = 0; i < UV.Length; i++)
            {
                UV[i] = new Vector2(0.5f,0.5f);
            }
            displayMesh.uv = UV;
            displayMesh.RecalculateNormals();
            MF.mesh = displayMesh;
            

        }

        public void OnBufferChange()
        {
            //Debug.Log("Buffer Change");
            switch (mapType)
            {
                case DisplayMapType.ALTITUDE_MAP:
                    AltitudeMap(); break;

                case DisplayMapType.CLOUD_MAP:
                    CloudMap(); break;

                case DisplayMapType.HEAT_MAP:
                    HeatMap(); break;

                case DisplayMapType.PRESSURE_MAP:
                    PressureMap(); break;

                case DisplayMapType.RAIN_MAP:
                    RainMap(); break;

                case DisplayMapType.WIND_MAP:
                    WindMap(); break;

                default:
                    HeatMap(); break;
            }



        }

        void HeatMap()
        {
            Vector2[] UV = new Vector2[displayMesh.vertexCount];
            foreach (KeyValuePair<Cell, WeatherCell> kvp in pSim.LiveMap[0])
            {
                //Vector2 = (0.5f, y)
                UV[kvp.Key.Index] = new Vector2(0.5f, (kvp.Value.Temperature + 0.05f) * 0.9f);
                
            }
            displayMesh.uv = UV;
        }
        void WindMap()
        {
            foreach (KeyValuePair<Cell, WeatherCell> kvp in pSim.LiveMap[0])
            {

            }
        }
        void PressureMap()
        {
            foreach (KeyValuePair<Cell, WeatherCell> kvp in pSim.LiveMap[0])
            {

            }
        }
        void AltitudeMap()
        {
            foreach (KeyValuePair<Cell, WeatherCell> kvp in pSim.LiveMap[0])
            {

            }
        }
        void CloudMap()
        {
            foreach (KeyValuePair<Cell, WeatherCell> kvp in pSim.LiveMap[0])
            {

            }
        }
        void RainMap()
        {
            foreach (KeyValuePair<Cell, WeatherCell> kvp in pSim.LiveMap[0])
            {

            }
        }

        void InitTexture()
        {
            displayTex = new Texture2D(1,128);
            for (int i = 0; i < 128; i++)
            {
                Color color = new Color(i/128f,0,1-(i/128f));
                displayTex.SetPixel(0, i, color);
                    
            }
            displayTex.Apply();
        }


    }
    public enum DisplayMapType
    {
        HEAT_MAP,
        WIND_MAP,
        PRESSURE_MAP,
        ALTITUDE_MAP,
        CLOUD_MAP,
        RAIN_MAP
    }

}
