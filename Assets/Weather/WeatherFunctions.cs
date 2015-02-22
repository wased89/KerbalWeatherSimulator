using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GeodesicGrid;

namespace Weather
{
    public class WeatherFunctions
    {

        public float getTemperature(Cell cell)
        {
            throw new NotImplementedException();
        }
        public float getLatitude(Cell cell)
        {
            return (float)(Math.Acos(cell.Position.y/ Math.Sqrt(cell.Position.x * cell.Position.x + cell.Position.z * cell.Position.z)) * 180 / Math.PI);
        }
        public float getLongitude(Cell cell)
        {
            return (float)(Math.Atan2(cell.Position.z, cell.Position.x) * 180 / Math.PI);
        }
        public float getAltitude(Cell cell)
        {
            throw new NotImplementedException();
        }

        public Vector3 getDirection(float latitude, float longitude)
        {
            Vector3 vector;
            vector.x = (float)(Math.Sin(latitude * Mathf.Deg2Rad) * Math.Cos(longitude * Mathf.Deg2Rad));
            vector.y = (float)Math.Cos(latitude * Mathf.Deg2Rad);
            vector.z = (float)(Math.Sin(latitude * Mathf.Deg2Rad) * Math.Sin(longitude * Mathf.Deg2Rad));

            return vector;
        }

        public static bool test()
        {
            return true;
        }

        public static float calculatePressure(PlanetSimulator pSim, Cell cell)
        {
            float pressure = 0f;
            
            return pressure;
        }

        public static float calculateDensity(PlanetSimulator pSim, Cell cell)
        {
            float density = 0;
            return density;
        }

        public float getHumidity(Cell cell)
        {
            throw new NotImplementedException();
        }
        public float getWindSpeed(Cell cell)
        {
            throw new NotImplementedException();
        }


    }
}
