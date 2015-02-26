using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GeodesicGrid;

namespace Weather
{
    public class Heating
    {
        private const float SBC = 0.000000056704f;
        private const double KSun = 3.17488630646816122100221E24;
        private const double LSun = 6.8930893418241730829829167104E24;



        public static void initShortwaves(PlanetSimulator pSim, Cell cell)
        {
            List<CellMap<WeatherCell>> buffer = pSim.LiveMap;

            for (int index = buffer.Count - 1; index > 0; index--)
            {

                WeatherCell temp = buffer[index][cell];

                if(index == buffer.Count-1) //is it top layer?
                {
                    if(false) //is not sunlight?
                    {
                        temp.SWAbsorbed = 0;
                        temp.SWReflected = 0;
                        temp.SWTransmitted = 0;
                    }
                    else
                    {
                        temp.SWReflected = pSim.bodyKSun * temp.Albedo;
                        temp.SWTransmitted = pSim.bodyKSun * temp.Transmissivity;
                        temp.SWAbsorbed = pSim.bodyKSun * (1 - temp.Albedo - temp.Transmissivity);
                    }
                    
                }
                if(index == 0) //bottom layer, has no transmit
                {
                    temp.SWReflected = buffer[index + 1][cell].SWTransmitted * temp.Albedo;
                    temp.SWTransmitted = 0;
                    temp.SWAbsorbed = buffer[index + 1][cell].SWTransmitted * (1 - temp.Albedo);
                }
                else //is middle layers
                {
                    temp.SWReflected = buffer[index + 1][cell].SWTransmitted * temp.Albedo;
                    temp.SWTransmitted = buffer[index + 1][cell].SWTransmitted * temp.Transmissivity;
                    temp.SWAbsorbed = buffer[index + 1][cell].SWTransmitted * (1 - temp.Albedo - temp.Transmissivity);
                }
                pSim.LiveMap[index][cell] = temp;

            }

            
        }

        public static bool isSunlight(PlanetSimulator pSim,int AltLayer, Cell cell)
        {
            if(getSunlightAngle(pSim, AltLayer, cell) > 91)
            {
                return false;
            }
            else
            {
                return true;
            }
            
        }

        public static float getSunlightAngle(PlanetSimulator pSim, int AltLayer,  Cell cell)
        {
            Vector3 sunPos = pSim.sunCallback();
            Vector3 cellPos = cell.Position;
            return Vector3.Angle(cellPos, sunPos);
        }

        public static double calculateBodyKSun(double radius)
        {
            double numb;
            numb = KSun / (4 * Math.PI * (radius * radius));
            return numb;
        }

        public static float ToTheFourth(float numb)
        {
            return numb * numb * numb * numb;
        }
    }
}
