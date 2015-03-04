using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalWeatherSimulator;
using KerbalWeatherSimulator;

namespace GUIUtils
{
    class DebugGUI
    {

        public DebugGUI(SunMove sunMove)
        {

        }

        public DebugGUI(PlanetSimulator pSim)
        {

        }

        public DebugGUI(Heating heat)
        {
            GameObject sun = GameObject.Find("Sun");
            GUILayout.Label("Body KSun: " + Heating.calculateBodyKSun(sun.transform.position.magnitude));
        }

    }
}
