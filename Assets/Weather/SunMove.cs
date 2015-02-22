using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalWeatherSystems;

namespace Weather
{
    class SunMove
    {
        GameObject sun;
        GameObject light;
        private Vector3 sunDirection;
        
        private float sunAngle;


        public SunMove()
        {
            sun = GameObject.Find("Sun");
            sun.renderer.material.color = new Color(255,255,0.5f);
            light = GameObject.Find("Directional light");
        }

        public void Update(Func<Vector3> func)
        {
            float sunSpeed = HeadMaster.sunRotationSpeed;
            sunAngle += Time.deltaTime * sunSpeed;
            double sunX = Math.Cos(sunAngle);
            double sunZ = Math.Sin(sunAngle);
            sunDirection = new Vector3((float)sunX, 0f, (float)sunZ);
            Vector3 newPosition = new Vector3((float)sunX * 7f, 0f, (float)sunZ * 7f);
            sun.transform.position = newPosition;

            light.transform.position = sun.transform.position;
            light.transform.rotation = Quaternion.LookRotation(-newPosition);
        }

        
    }
}
