using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GeodesicGrid;

namespace KerbalWeatherSimulator
{
    public class Heating
    {
        private const float SBC = 0.000000056704f;
        private const double KSun = 3.17488630646816122100221E24;
        private const double LSun = 6.8930893418241730829829167104E24;



        public static void InitShortwaves(PlanetSimulator pSim, Cell cell)
        {
            for (int index = pSim.LiveMap.Count - 1; index > 0; index--)
            {
                WeatherCell temp = pSim.LiveMap[index][cell];
                float SunriseFactor = (float)(Mathf.Cos(WeatherFunctions.getLatitude(cell) * Mathf.Deg2Rad) +
                    Mathf.Cos(getSunlightAngle(pSim, index, cell) * Mathf.Deg2Rad)) / 2f;
                if (index == pSim.LiveMap.Count - 1) //Is it top layer?
                {
                    //check for sunlight
                    if (isSunlight(pSim, index, cell))
                    {
                        float bodyKSun = pSim.bodyKSun * SunriseFactor;
                        temp.SWReflected = bodyKSun * temp.Albedo;
                        temp.SWTransmitted = bodyKSun * temp.Transmissivity;
                        temp.SWAbsorbed = bodyKSun *
                            (1 - temp.Albedo - temp.Transmissivity);
                    }
                    else
                    {
                        temp.SWAbsorbed = 0f;
                        temp.SWReflected = 0f;
                        temp.SWTransmitted = 0f;
                    }
                }
                else if (index == 0) //is it bottom layer? No transmit
                {
                    temp.SWReflected = pSim.BufferMap[index + 1][cell].SWTransmitted * temp.Albedo;
                    temp.SWTransmitted = 0f;
                    temp.SWAbsorbed = pSim.BufferMap[index + 1][cell].SWTransmitted *
                        (1 - temp.Albedo - temp.Transmissivity);
                }
                else
                {
                    temp.SWReflected = pSim.BufferMap[index + 1][cell].SWTransmitted * temp.Albedo;
                    temp.SWTransmitted = pSim.BufferMap[index + 1][cell].SWTransmitted * temp.Transmissivity;
                    temp.SWAbsorbed = pSim.BufferMap[index + 1][cell].SWTransmitted *
                        (1 - temp.Albedo - temp.Transmissivity);
                }
            }
        }
        
        public static void CalculateShortwaves(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            WeatherCell temp = pSim.LiveMap[AltLayer][cell];
            if(AltLayer == pSim.LiveMap.Count -1) //Is it top layer?
            {
                float SunriseFactor = (float)(Mathf.Cos(WeatherFunctions.getLatitude(cell) * Mathf.Deg2Rad) + 
                    Mathf.Cos(getSunlightAngle(pSim, AltLayer, cell) * Mathf.Deg2Rad)) / 2f;
                //Do check to see if top layer is in sunlight
                if(isSunlight(pSim, AltLayer, cell))
                {
                    float bodyKSun = pSim.bodyKSun * SunriseFactor;
                    temp.SWReflected = bodyKSun * temp.Albedo;
                    temp.SWTransmitted = bodyKSun * temp.Transmissivity;
                    temp.SWAbsorbed = bodyKSun *
                        (1 - temp.Albedo - temp.Transmissivity);
                }
                //Else, it's danky dark and this is where the sun don't shine
                else
                {
                    temp.SWAbsorbed = 0;
                    temp.SWReflected = 0;
                    temp.SWTransmitted = 0;
                }
            }
            else if(AltLayer == 0) //Is it bottom layer? No transmit
            {
                temp.SWReflected = pSim.BufferMap[AltLayer + 1][cell].SWTransmitted * temp.Albedo;
                temp.SWTransmitted = 0f;
                temp.SWAbsorbed = pSim.BufferMap[AltLayer + 1][cell].SWTransmitted *
                    (1 - temp.Albedo - temp.Transmissivity);
            }
            else //it's middle layers
            {
                temp.SWReflected = pSim.BufferMap[AltLayer + 1][cell].SWTransmitted * temp.Albedo;
                temp.SWTransmitted = pSim.BufferMap[AltLayer + 1][cell].SWTransmitted * temp.Transmissivity;
                temp.SWAbsorbed = pSim.BufferMap[AltLayer + 1][cell].SWTransmitted *
                    (1 - temp.Albedo - temp.Transmissivity);
            }
            pSim.BufferMap[AltLayer][cell] = temp;
        }


        internal static void CalculateLongwaves(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            //calc emissions
            //calc transmissions
            //calc incoming
            //calc temp
            CalculateEmissions(pSim, AltLayer, cell);
            CalculateTransmissions(pSim, AltLayer, cell);
            CalculateIncoming(pSim, AltLayer, cell);
            
        }

        internal static void CalculateEmissions(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            WeatherCell temp = pSim.BufferMap[AltLayer][cell];
            temp.LWOut = temp.Emissivity * SBC * ToTheFourth(temp.Temperature);
        }
        internal static void CalculateTransmissions(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            WeatherCell temp = pSim.BufferMap[AltLayer][cell];

            if(AltLayer == 0) //ground layer, no transmit
            {
                temp.LWTransmit = 0;
            }
            else if(AltLayer == 1) //layer above ground
            {
                temp.LWTransmit = pSim.BufferMap[AltLayer - 1][cell].LWOut * (1 - temp.Emissivity);
            }
            else if(AltLayer<= pSim.LiveMap.Count -2) //middle layers
            {
                temp.LWOut = (1 - temp.Emissivity) *
                    (pSim.BufferMap[AltLayer - 1][cell].LWOut + pSim.BufferMap[AltLayer - 1][cell].LWTransmit);
            }
            else //Top layer 
            {
                temp.LWOut = (1 - temp.Emissivity) *
                    (pSim.BufferMap[AltLayer - 1][cell].LWOut + pSim.BufferMap[AltLayer - 1][cell].LWTransmit);
            }

        }
        internal static void CalculateIncoming(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            WeatherCell temp = pSim.BufferMap[AltLayer][cell];

            if (AltLayer == 0) //ground layer
            {
                temp.LWIn = temp.Emissivity * 
                    (pSim.BufferMap[AltLayer + 1][cell].LWOut + pSim.BufferMap[AltLayer + 1][cell].LWIn);
            }
            else if (AltLayer == pSim.LiveMap.Count - 2) //middle layers
            {
                temp.LWIn = temp.Emissivity * 
                    ((pSim.BufferMap[AltLayer - 1][cell].LWOut + pSim.BufferMap[AltLayer - 1][cell].LWTransmit) +
                    (pSim.BufferMap[AltLayer + 1][cell].LWOut + pSim.BufferMap[AltLayer + 1][cell].LWTransmit));
            }
            else //top layer
            {
                temp.LWIn = temp.Emissivity * (pSim.BufferMap[AltLayer - 1][cell].LWOut + pSim.BufferMap[AltLayer - 1][cell].LWTransmit);
            }


        }

        internal static float CalculateTemperature(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            //Kin + Lin = Lout;
            //Lout = e,layer * SBC * T,layer^4
            //t^4 = Lout/ e,layer * SBC
            //t = 4throot(Lout/e,layer * SBC)

            return Mathf.Pow((float)(((pSim.BufferMap[AltLayer][cell].SWAbsorbed + pSim.BufferMap[AltLayer][cell].LWIn) / 2) /
                (pSim.BufferMap[AltLayer][cell].Emissivity * SBC)), 0.25f);
        }

        internal static void InitLongwaves(PlanetSimulator pSim, Cell cell)
        {
            for (int index = 0; index <= pSim.LiveMap.Count - 1; index++)
            {
                WeatherCell temp = pSim.LiveMap[index][cell];
                if(index == 0) //is surface layer
                {
                    temp.LWOut = temp.Emissivity * SBC * ToTheFourth(temp.Temperature);
                    temp.LWIn = 0;
                    temp.LWTransmit = 0;
                }
                else if(index == 1) //layer above surface
                {
                    temp.LWIn = pSim.LiveMap[index - 1][cell].LWOut * temp.Emissivity;
                    temp.LWTransmit = pSim.LiveMap[index -1][cell].LWOut * (1 - temp.Emissivity);
                    temp.LWOut = temp.Emissivity * SBC * ToTheFourth(temp.Temperature);
                }
                else if(index < pSim.LiveMap.Count - 2) //middle layers
                {
                    temp.LWIn = temp.Emissivity * 
                        (pSim.LiveMap[index - 1][cell].LWOut + pSim.LiveMap[index - 1][cell].LWTransmit);
                    temp.LWTransmit = (1 - temp.Emissivity) * 
                        (pSim.LiveMap[index - 1][cell].LWOut + pSim.LiveMap[index -1][cell].LWTransmit);
                    temp.LWOut = temp.Emissivity * SBC * ToTheFourth(temp.Temperature);
                }
                else//top layer
                {
                    temp.LWIn = temp.Emissivity *
                        (pSim.LiveMap[index - 1][cell].LWOut + pSim.LiveMap[index - 1][cell].LWTransmit);
                    temp.LWTransmit = (1 - temp.Emissivity) *
                        (pSim.LiveMap[index - 1][cell].LWOut + pSim.LiveMap[index - 1][cell].LWTransmit);
                    temp.LWOut = temp.Emissivity * SBC * ToTheFourth(temp.Temperature);
                }
            }
        }

        public static bool isSunlight(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            if(getSunlightAngle(pSim, AltLayer, cell) > 90)
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
            Vector3 sunPos = pSim.sunDir;
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
