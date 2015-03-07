using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalWeatherSimulator;

namespace KerbalWeatherSimulator
{
    class SunMove
    {
        GameObject sun;
        GameObject light;
        
        
        private float sunAngle;


        public SunMove()
        {
            sun = GameObject.Find("Sun");
            sun.renderer.material.color = new Color(255,255,0.5f);
            light = GameObject.Find("Directional light");
        }

        public void Update()
        {
            float sunSpeed = HeadMaster.sunRotationSpeed;
            sunAngle += Time.deltaTime * sunSpeed;
            double sunY = Math.Sin(sunAngle) * 0.25f; ;
            double sunX = Math.Cos(sunAngle) * Math.Cos(sunY);
            double sunZ = Math.Sin(sunAngle) * Math.Cos(sunY);
            
            Vector3 newPosition = new Vector3((float)sunX * 5f, 0f, (float)sunZ * 5f);
            sun.transform.position = newPosition;

            light.transform.position = sun.transform.position;
            light.transform.rotation = Quaternion.LookRotation(-newPosition);
        }

        
    }
}
