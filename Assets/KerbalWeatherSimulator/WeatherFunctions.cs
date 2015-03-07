using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GeodesicGrid;

namespace KerbalWeatherSimulator
{
    public class WeatherFunctions
    {

        public static float getTemperature(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            return pSim.LiveMap[AltLayer][cell].Temperature;
        }
        public static float getLatitude(Cell cell)
        {
            return (float)(Math.Acos(cell.Position.y) * (180 / Math.PI))-90f;
        }
        public static float getLongitude(Cell cell)
        {
            return (float)(Math.Atan2(cell.Position.z, cell.Position.x) * (180 / Math.PI));
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

        

        public static Vector3 CalculateWindVector(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            //Debug.Log("Calcing wind vector");
            Vector3 resultant = Vector3.zero;
            foreach (Cell neighbour in cell.GetNeighbors(pSim.level))
            {
                float deltaPressure = pSim.LiveMap[AltLayer][cell].Pressure - pSim.LiveMap[AltLayer][neighbour].Pressure;
                
                Vector3 cellVector = cell.Position - neighbour.Position;
                
                float neighbourDistance = cellVector.magnitude;
                cellVector.Normalize();
                if (deltaPressure == 0f)
                {
                    continue;
                }
                
                float acc = (-1 / pSim.LiveMap[AltLayer][cell].Density) * (deltaPressure / neighbourDistance);
                
                //v = a * sqrt(2d/a)
                float windSpeed = acc * Mathf.Sqrt((2 * neighbourDistance) / Mathf.Abs(acc));
                
                //divide by 2 because opposite shit. Trust me, it is.
                Vector3 windVector = new Vector3(cellVector.x * windSpeed / 2f, cellVector.y * windSpeed / 2f, cellVector.z * windSpeed / 2f);
                
                //Apply Coriolis to windVector
                Vector3 corAcc = 2 * Vector3.Cross(windVector, pSim.angularVelocity + new Vector3(0f, Mathf.Cos(WeatherFunctions.getLatitude(cell)), Mathf.Sin(WeatherFunctions.getLatitude(cell))));
                
                windVector = windVector + corAcc;
                resultant += windVector;
            }
            
            return resultant;
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

        public static float newCalculatePressure(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            //p1 * t2 = p2 * t1;
            //p2 = (p1 * t2)/t1;

            float pressure = (pSim.LiveMap[AltLayer][cell].Pressure * pSim.BufferMap[AltLayer][cell].Temperature)
                / pSim.LiveMap[AltLayer][cell].Temperature;
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
        public static float getCellAlbedo(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            return 0f;
        }


    }
}
