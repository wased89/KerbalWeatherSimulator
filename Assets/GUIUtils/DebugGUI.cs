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

        private static Rect mainWindow = new Rect(0,0,100,50);
        private static Rect basicDataWindow = new Rect(0,0,150,150);

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
        }

        public DebugGUI(Heating heat)
        {
            GameObject sun = GameObject.Find("Sun");
            GUILayout.Label("Body KSun: " + Heating.calculateBodyKSun(sun.transform.position.magnitude));
        }

        public void Update()
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hoverCell = Cell.Raycast(ray, pSim.level, new BoundsMap(x => 1, pSim.level), heightAt());

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

        void BasicDataWindow(int windowID)
        {
            if(hoverCell != null)
            {
                GUILayout.Label("Layer: " + 0);
                GUILayout.Label("Cell: " + hoverCell.Value.Index);
                GUILayout.Label("Temp: " + pSim.LiveMap[0][hoverCell.Value].Temperature);
                GUILayout.Label("Press: " + pSim.LiveMap[0][hoverCell.Value].Pressure);
                GUILayout.Label("Dens: " + pSim.LiveMap[0][hoverCell.Value].Density);
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
