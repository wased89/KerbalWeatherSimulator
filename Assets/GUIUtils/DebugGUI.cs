using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalWeatherSimulator;
using ObjectSim;
using GeodesicGrid;

namespace GUIUtils
{
    class DebugGUI
    {
        PlanetSimulator pSim;
        Cell? hoverCell;
        Cell? lastKnownCell;
        BoundsMap bMap;
        private static Rect mainWindow = new Rect(0,0,100,50);
        private static Rect basicDataWindow = new Rect(0,0,150,425);

        int mainWindowID;
        int basicWindowID;

        bool showMainWindow;
        bool showBasicWindow;

        public DebugGUI(SunMove sunMove)
        {

        }

        public DebugGUI(PlanetSimulator pSim)
        {
            this.pSim = pSim;
            mainWindowID = Guid.NewGuid().GetHashCode();
            basicWindowID = Guid.NewGuid().GetHashCode();
            bMap = new BoundsMap(x => 1, pSim.level);
        }

        public DebugGUI(Heating heat)
        {
            GameObject sun = GameObject.Find("Sun");
            GUILayout.Label("Body KSun: " + Heating.calculateBodyKSun(sun.transform.position.magnitude));
        }

        public void Update()
        {
            GameObject urf = GameObject.Find("Earth");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hoverCell = Cell.Raycast(ray, pSim.level, bMap, heightAt(), urf.transform);

            //if (hoverCell != null) { Debug.Log("HoverCell index: " + hoverCell.Value.Index); }
        }
        public void OnGUI()
        {

            mainWindow = GUILayout.Window(mainWindowID, mainWindow, MainWindow, "Main Window~");

            if (showBasicWindow)
            {
                basicDataWindow = GUI.Window(basicWindowID, basicDataWindow, BasicDataWindow, "Basic Data~");
            }

            
        }
        void MainWindow(int windowID)
        {
            GUILayout.BeginVertical();
            showBasicWindow = GUILayout.Toggle(showBasicWindow, "Basic~");
            GUILayout.EndVertical();
            
        }

        int layer = 0;
        
        void BasicDataWindow(int windowID)
        {
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Layer ++") && layer < pSim.LiveMap.Count - 1) { layer++; };
            if (GUILayout.Button("Layer --") && layer > 0) { layer--; }
            GUILayout.EndHorizontal();
            GUILayout.Label("Layer: " + layer);
            
            if(hoverCell != null)
            {

                lastKnownCell = hoverCell.Value;
                
                
            }
            if(lastKnownCell != null)
            {
                GUILayout.Label("Cell: " + lastKnownCell.Value.Index);
                GUILayout.Label("Temp: " + (pSim.LiveMap[layer][lastKnownCell.Value].Temperature - 273.15f));
                GUILayout.Label("Press: " + pSim.LiveMap[layer][lastKnownCell.Value].Pressure);
                GUILayout.Label("Dens: " + pSim.LiveMap[layer][lastKnownCell.Value].Density);
                GUILayout.Label("Trans: " + pSim.LiveMap[layer][lastKnownCell.Value].Transmissivity);
                GUILayout.Label("Emiss: " + pSim.LiveMap[layer][lastKnownCell.Value].Emissivity);
                GUILayout.Label("SWAbs: " + pSim.LiveMap[layer][lastKnownCell.Value].SWAbsorbed);
                GUILayout.Label("SWRef: " + pSim.LiveMap[layer][lastKnownCell.Value].SWReflected);
                GUILayout.Label("SWTrans: " + pSim.LiveMap[layer][lastKnownCell.Value].SWTransmitted);
                GUILayout.Label("LWIn: " + pSim.LiveMap[layer][lastKnownCell.Value].LWIn);
                GUILayout.Label("LWOut: " + pSim.LiveMap[layer][lastKnownCell.Value].LWOut);
                GUILayout.Label("LWTransmit: " + pSim.LiveMap[layer][lastKnownCell.Value].LWTransmit);
                GUILayout.Label("WindDir: " + pSim.LiveMap[layer][lastKnownCell.Value].WindDirection);
            }
             
            GUI.DragWindow();
        }

        public Func<Cell, float> heightAt()
        {
            Func<Cell, float> heightRatioAt;
            heightRatioAt = c => 1;
            return heightRatioAt;
        }
    }
}
