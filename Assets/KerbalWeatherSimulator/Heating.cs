using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GeodesicGrid;

namespace KerbalWeatherSimulator
{
    //run the radiative model, then run convection after it
    //heat transferred = h*A*(deltaT)
    //h = heat transfer coefficient
    //A = contact area
    //deltaT = temperature delta
    //this is a rolling update
    //calc h each time
    //use newton's law of cooling:
    //dQ/dt = h*a*(T(t) - Tenv) = h*A*Tgradient(t)

    //step1: calc heat transfer coefficient
    //step 2: calc temperature gradient
    //step 3: 


    public class Heating
    {
        private const float SBC = 0.000000056704f;
        private const double KSun = 3.17488630646816122100221E24;
        private const double LSun = 6.8930893418241730829829167104E24;



        public static void InitShortwaves(PlanetSimulator pSim, Cell cell)
        {
            //Debug.Log("Init shortwaves");
            for (int index = pSim.LiveMap.Count - 1; index > 0; index--)
            {
                
                WeatherCell temp = pSim.LiveMap[index][cell];
                if (index == pSim.LiveMap.Count - 1) //Is it top layer?
                {
                    float SunriseFactor = (float)(Mathf.Cos(WeatherFunctions.getLatitude(cell) * Mathf.Deg2Rad) +
                    Mathf.Cos(getSunlightAngle(pSim, index, cell) * Mathf.Deg2Rad)) / 2f;
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
                    temp.SWReflected = pSim.LiveMap[index + 1][cell].SWTransmitted * temp.Albedo;
                    temp.SWTransmitted = 0f;
                    temp.SWAbsorbed = pSim.LiveMap[index + 1][cell].SWTransmitted *
                        (1 - temp.Albedo - temp.Transmissivity);
                }
                else
                {
                    temp.SWReflected = pSim.LiveMap[index + 1][cell].SWTransmitted * temp.Albedo;
                    temp.SWTransmitted = pSim.LiveMap[index + 1][cell].SWTransmitted * temp.Transmissivity;
                    temp.SWAbsorbed = pSim.LiveMap[index + 1][cell].SWTransmitted *
                        (1 - temp.Albedo - temp.Transmissivity);
                }
                pSim.LiveMap[index][cell] = temp;
            }
        }
        
        public static void CalculateShortwaves(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            WeatherCell temp = pSim.BufferMap[AltLayer][cell];
            if(AltLayer == pSim.BufferMap.Count -1) //Is it top layer?
            {
                float SunriseFactor = (float)(Mathf.Cos(WeatherFunctions.getLatitude(cell) * Mathf.Deg2Rad) + 
                    Mathf.Cos(getSunlightAngle(pSim, AltLayer, cell) * Mathf.Deg2Rad)) / 2f;
                //Debug.Log("Sunrise Factor: " + SunriseFactor); //checks out
                //Do check to see if top layer is in sunlight
                if(isSunlight(pSim, AltLayer, cell))
                {
                    float bodyKSun = pSim.bodyKSun * SunriseFactor;
                    //Debug.Log("bodyKSun: " + bodyKSun);
                    temp.SWReflected = bodyKSun * temp.Albedo;
                    temp.SWTransmitted = bodyKSun * temp.Transmissivity;
                    //Debug.Log(temp.SWTransmitted); //top layer gives real values
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
                    (1 - temp.Albedo);
                
                //Debug.Log(pSim.BufferMap[AltLayer +1][cell].SWTransmitted); //Gives 0
            }
            else //it's middle layers
            {
                temp.SWReflected = pSim.BufferMap[AltLayer + 1][cell].SWTransmitted * temp.Albedo;
                temp.SWTransmitted = pSim.BufferMap[AltLayer + 1][cell].SWTransmitted * temp.Transmissivity;
                temp.SWAbsorbed = pSim.BufferMap[AltLayer + 1][cell].SWTransmitted *
                    (1 - temp.Albedo - temp.Transmissivity);
                
                //Debug.Log("Layer: "+ AltLayer + ", " + pSim.BufferMap[pSim.LiveMap.Count-1][cell].SWTransmitted); //gives 0
            }
            //pSim.LiveMap[AltLayer][cell] = temp;
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
            pSim.BufferMap[AltLayer][cell] = temp;
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
            pSim.BufferMap[AltLayer][cell] = temp;
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
                temp.LWIn = temp.Emissivity * 
                    (pSim.BufferMap[AltLayer - 1][cell].LWOut + pSim.BufferMap[AltLayer - 1][cell].LWTransmit);
            }
            pSim.BufferMap[AltLayer][cell] = temp;

        }

        internal static float CalculateTemperature(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            //Kin + Lin = Lout;
            //Lout = e,layer * SBC * T,layer^4
            //t^4 = Lout/ e,layer * SBC
            //t = 4throot(Lout/e,layer * SBC)
            CalculateShortwaves(pSim, AltLayer, cell);
            CalculateLongwaves(pSim, AltLayer, cell);
            //Debug.Log("SWAbs: " + pSim.BufferMap[AltLayer][cell].SWAbsorbed);
            //return Mathf.Pow((float)(((pSim.BufferMap[AltLayer][cell].LWOut) /
                //(pSim.LiveMap[AltLayer][cell].Emissivity * SBC))), 0.25f);
            return Mathf.Pow((float)(((pSim.BufferMap[AltLayer][cell].SWAbsorbed + pSim.BufferMap[AltLayer][cell].LWIn) / 2.0f) /
                (pSim.LiveMap[AltLayer][cell].Emissivity * SBC)), 0.25f);
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
                pSim.LiveMap[index][cell] = temp;
            }
        }

        public static float calculateAlbedo(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            return 0.30f * Mathf.Pow(0.75f, AltLayer);
        }
        public static float calculateEmissivity(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            if(pSim.LiveMap[0][cell].isOcean)
            {
                return 0.96f;
            }
            else if (WeatherFunctions.getLatitude(cell) > 60 || WeatherFunctions.getLatitude(cell) < -60 && pSim.LiveMap[0][cell].isOcean == false)
            {
                return 0.97f;

            }
            else
            {
                return 0.92f;
            }
            
        }
        internal static float calculateTransmissivity(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            //Beers-lambert law covers transmissivity
            //basically: transmitting rad = starting rad * e^(-m*optical depth)
            //rearranged: transmissivity = e^(-m * optical depth)
            //-m is the optical airmass
            //which is a scaling parameter based on the amount of air that the ray will travel through
            //optical depth is the "opacity" of the air and is dependant on composition
            //optical airmass is dependant on the thickness of the layers, and pressure

            float opticalDepth = calculateOpticalDepth(pSim, cell);
            float opticalAirMass = calculateAtmosphericPathLength(pSim, AltLayer, cell);
            float T = Mathf.Pow((float)Math.E, (-opticalAirMass * opticalDepth));
            return T * (1 - pSim.LiveMap[AltLayer][cell].Albedo);
        }

        internal static float calculateAtmosphericPathLength(PlanetSimulator pSim, int AltLayer, Cell cell)
        {

            float zenithAngle = getSunlightAngle(pSim, AltLayer, cell) * Mathf.Deg2Rad;
            float ymax = pSim.LiveMap[pSim.LiveMap.Count - 1][cell].Altitude + 2500f;
            float stuff = (float)Math.Sqrt(
                (((pSim.bodyRadius + pSim.LiveMap[AltLayer][cell].Altitude) / ymax) * ((pSim.bodyRadius + pSim.LiveMap[AltLayer][cell].Altitude) / ymax)) *
                (Mathf.Cos(zenithAngle) * Mathf.Cos(zenithAngle)) +
                ((2 * pSim.bodyRadius) / (ymax * ymax)) *
                (ymax - pSim.LiveMap[AltLayer][cell].Altitude) -
                ((ymax / ymax) * (ymax / ymax)) + 1 -
                ((pSim.bodyRadius + pSim.LiveMap[AltLayer][cell].Altitude) / ymax) *
                (Mathf.Cos(zenithAngle))
                );
            return stuff;
        }
        internal static float calculateOpticalDepth(PlanetSimulator pSim, Cell cell)
        {
            float opticalDepth = 0.02f; //Original: 0.2f
            return opticalDepth;
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

        public static double calculateBodyKSun(double orbitRadius)
        {
            double numb;
            numb = KSun / (4 * Math.PI * (orbitRadius * orbitRadius));
            return numb;
        }

        public static float ToTheFourth(float numb)
        {
            return numb * numb * numb * numb;
        }
    }
}
