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

        public static float getTemperature(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            return pSim.LiveMap[AltLayer][cell].Temperature;
        }
        public static float getLatitude(Cell cell)
        {
            return (float)(Math.Acos(cell.Position.y/ Math.Sqrt(cell.Position.x * cell.Position.x + cell.Position.z * cell.Position.z)) * 180 / Math.PI);
        }
        public static float getLongitude(Cell cell)
        {
            return (float)(Math.Atan2(cell.Position.z, cell.Position.x) * 180 / Math.PI);
        }
        public static float getAltitude(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            return pSim.LiveMap[AltLayer][cell].Altitude;
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

        public static float calculatePressure(float basePressure, float TLR, float temperature, float AOC, float HOL, float geeASL, float MMOA)
        {
            //Things we need: base pressure of level, Temp Lapse Rate, Temperature, Altitude of cell,
            //height of layer, Gravity at sea level, Molar mass of air, universal gas constant for air = 8.31432

            //Possible causes of error: temperature is 0 and TLR is 0, resulting in NaN
            //However temperature is in Kelvins, so that shouldn't ever happen

            float pressure = basePressure * Mathf.Pow((temperature / (temperature + TLR) * (AOC + HOL)),((geeASL * MMOA) / (8.31432f * TLR)));
            
            return pressure;
        }

        public static float calculateDensity(float baseDensity, float TLR, float temperature, float AOC, float HOL, float geeASL, float MMOA)
        {
            //Things we need: base density of level, temp, TLR, altitude, height of cell, geeASL, Molar mass of air
            //universal gas constant for air
            float density = baseDensity * Mathf.Pow((((temperature + TLR) * ((AOC + HOL)-AOC)) / temperature),((-(geeASL * MMOA) / (8.31432f * TLR)) -1f));
            return density;
        }

        public static float getHumidity(Cell cell)
        {
            throw new NotImplementedException();
        }
        public float getWindSpeed(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            return pSim.LiveMap[AltLayer][cell].WindDirection.magnitude;
        }


    }
}
