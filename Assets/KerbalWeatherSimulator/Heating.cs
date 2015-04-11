using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GeodesicGrid;
using KerbalWeatherSimulator;

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

    //possible equation for h:
    //h = (Nu)(k)/(D) where Nu comes from the Dittus-Boelter equation, k is the thermal conductivity, D is distance
    //Nu = 0.023 Re^0.8 Pr^0.4 where the exp of Pr is 0.3 if the liquid is cooling, 0.4 if heating.
    //Re is the Reynolds number and Pr is the Prandtl number.
    //Re = DVp/u where u is the viscosity of the fluid, D is distance, and V is the relative velocity, p is density
    //Pr = v/alpha = u Cp / k where Cp is the heat capacity of the fluid in kj/kg-K

    //step1: calc temperature gradient
    //step 2: calculate Reynolds number
    //step 3: calculate Prandtl number
    //step 4: calcualte Nusselt number
    //step 5: calculate heat transfer coefficient
    //step 6: calculate final result
    //step 7: do the shit needed to add it to the temp of the cell


    //What if doing the cellVector and the windVector for both cells in question and comparing them?
    //h * A * Tgrad where Tgrad is measured over deltaT, deltaT *should* be the time since last updating the cell.
    //Q = m *c * deltaT, where m = mass of air, c = heat capacity, deltaT = change in temp
    // h * A * Tgrad = m * c * deltaT, solve for deltaT
    //deltaT = ((h * A * Tgrad)/m)/c
    //add deltaT to final temp


    public class Heating
    {
        private const float SBC = 0.000000056704f;
        private const double KSun = 3.17488630646816122100221E24;
        private const double LSun = 6.8930893418241730829829167104E24;
        internal static double atmoViscAtRef0C = 1.73620646931577869E-5;
        internal static double atmoHeatCapacity = 1.01; //1.01kj/kg-K
        internal static double atmoThermalConductivity = 0.024; //W/m-K



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
                    if (WeatherFunctions.isSunlight(pSim, index, cell))
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
                if(WeatherFunctions.isSunlight(pSim, AltLayer, cell))
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
            if(pSim.timesLeft > 0)
            {
                return (Mathf.Pow((float)(((pSim.BufferMap[AltLayer][cell].SWAbsorbed + pSim.BufferMap[AltLayer][cell].LWIn) / 2.0f) /
                (pSim.LiveMap[AltLayer][cell].Emissivity * SBC)), 0.25f));
            }
            else
            {
                return (Mathf.Pow((float)(((pSim.BufferMap[AltLayer][cell].SWAbsorbed + pSim.BufferMap[AltLayer][cell].LWIn) / 2.0f) /
                (pSim.LiveMap[AltLayer][cell].Emissivity * SBC)), 0.25f))+calculateFinalTempDelta(pSim, AltLayer, cell);
            }
            
        }

        internal static void InitLongwaves(PlanetSimulator pSim, Cell cell)
        {
            for (int index = 0; index <= pSim.LiveMap.Count - 1; index++)
            {
                WeatherCell temp = pSim.BufferMap[index][cell];
                if(index == 0) //is surface layer
                {
                    temp.LWOut = temp.Emissivity * SBC * ToTheFourth(temp.Temperature);
                    temp.LWIn = 0;
                    temp.LWTransmit = 0;
                }
                else if(index == 1) //layer above surface
                {
                    temp.LWIn = pSim.BufferMap[index - 1][cell].LWOut * temp.Emissivity;
                    temp.LWTransmit = pSim.BufferMap[index -1][cell].LWOut * (1 - temp.Emissivity);
                    temp.LWOut = temp.Emissivity * SBC * ToTheFourth(temp.Temperature);
                }
                else if(index < pSim.LiveMap.Count - 2) //middle layers
                {
                    temp.LWIn = temp.Emissivity * 
                        (pSim.BufferMap[index - 1][cell].LWOut + pSim.BufferMap[index - 1][cell].LWTransmit);
                    temp.LWTransmit = (1 - temp.Emissivity) * 
                        (pSim.BufferMap[index - 1][cell].LWOut + pSim.BufferMap[index -1][cell].LWTransmit);
                    temp.LWOut = temp.Emissivity * SBC * ToTheFourth(temp.Temperature);
                }
                else//top layer
                {
                    temp.LWIn = temp.Emissivity *
                        (pSim.BufferMap[index - 1][cell].LWOut + pSim.BufferMap[index - 1][cell].LWTransmit);
                    temp.LWTransmit = (1 - temp.Emissivity) *
                        (pSim.BufferMap[index - 1][cell].LWOut + pSim.BufferMap[index - 1][cell].LWTransmit);
                    temp.LWOut = temp.Emissivity * SBC * ToTheFourth(temp.Temperature);
                }
                pSim.BufferMap[index][cell] = temp;
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

        internal static float calculateFinalTempDelta(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            //deltaT = ((h * A * Tgrad)/m)/c
            float deltaT;
            float HF = calculateNetHeatFlow(pSim, AltLayer, cell);
            float m = 100f;
            float c = (float)atmoHeatCapacity;
            deltaT = (HF / m) / c;
            return deltaT;
            
        }

        internal static float calculateNetHeatFlow(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            //HF = h * A * Tgrad
            float HF = 0;
            foreach(Cell neighbor in cell.GetNeighbors(pSim.level))
            {
                float Tgrad = calculateTemperatureGradient(pSim, AltLayer, AltLayer, cell, neighbor);
                float h = calculateHeatTransferCoefficient(pSim, AltLayer, cell, neighbor);
                float A = pSim.LiveMap[AltLayer][cell].Height * 2500;
                
                HF += h * A * Tgrad;
            }
            
            return HF; //returns W/s
        }

        internal static float calculateTemperatureGradient(PlanetSimulator pSim, int AltLayerA, int AltLayerB, Cell cellA, Cell cellB)
        {
            float Tgrad = pSim.LiveMap[AltLayerA][cellA].Temperature - pSim.LiveMap[AltLayerB][cellB].Temperature;
            
            
            
            return Tgrad;
        }
        internal static float calculateHeatTransferCoefficient(PlanetSimulator pSim, int AltLayer, Cell cellA,Cell cellB)
        {
            //h = (Nu)(k)/D
            //Nu = Nusselt number, k = thermal conductivity, D = characteristic length parameter, such as diameter for flow through a pipe.
            float tg = calculateTemperatureGradient(pSim, AltLayer, AltLayer, cellA, cellB);
            float cellHeight = pSim.LiveMap[AltLayer][cellA].Height;
            float pointWidth = 2500f; //the width of the intersecting area between two cells
            float D = Mathf.Sqrt((pointWidth * cellHeight));
            float Nu;
            float h;
            float k = (float)Heating.atmoThermalConductivity;
            if (tg > 0) //is cooling
            {
                Nu = calculateNusseltNumber(pSim, AltLayer, cellA,cellB, true);
                h = (Nu * k) / D;

            }
            else
            {
                Nu = calculateNusseltNumber(pSim, AltLayer, cellA, cellB, false);
                h = (Nu * k) / D;
            }
            

            return h;
        }
        internal static float calculateNusseltNumber(PlanetSimulator pSim, int AltLayer, Cell cellA, Cell cellB, bool isCooling)
        {
            float Re = calculateReynoldsNumber(pSim, AltLayer, cellA, cellB);
            float Pr = calculatePrandtlNumber(pSim, AltLayer, cellA);
            float Nu;
            if (isCooling)
            {
                Nu = 0.023f * Mathf.Pow(Re, 0.8f) * Mathf.Pow(Pr, 0.3f);
            }
            else
            {
                Nu = 0.023f * Mathf.Pow(Re, 0.8f) * Mathf.Pow(Pr, 0.4f);
            }
            
            return Nu;
        }
        internal static float calculateReynoldsNumber(PlanetSimulator pSim, int AltLayer, Cell cellA, Cell cellB)
        {
            //Re = DVp/u
            //D = Distance parameter thing, V = velocity, p = density, u = viscosity
            float D = Mathf.Sqrt(pSim.LiveMap[AltLayer][cellA].Height * 2500);
            Vector3 cellVector = cellA.Position - cellB.Position;
            float v = pSim.LiveMap[AltLayer][cellB].WindDirection.magnitude;
            
            float V =  v * Mathf.Cos((Vector3.Dot(cellVector, pSim.LiveMap[AltLayer][cellB].WindDirection)));
            if(float.IsNaN(V) && cellA.Index == 10)
            {
                Debug.Log("Velocity is NaN");
            }
            float p = pSim.LiveMap[AltLayer][cellA].Density;
            float u = calculateViscosity(pSim, AltLayer, cellA);
            float Re = (D * V * p) / u;
            
           
            return Re;
        }
        internal static float calculatePrandtlNumber(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            //PN = v*Cp/k
            //v is viscosity, Cp is specific heat capacity, k is thermal conductivity
            float v = calculateViscosity(pSim, AltLayer, cell);
            float Cp = (float)Heating.atmoHeatCapacity;
            float k = (float)Heating.atmoThermalConductivity;
            float PN = (v * Cp) / k;
            
            return PN;
        }
        internal static float calculateViscosity(PlanetSimulator pSim, int AltLayer, Cell cell)
        {
            //u/u0 = (T/T0)^0.7
            //u = u0 * ((T/273.15)^0.7)
            float T = pSim.LiveMap[AltLayer][cell].Temperature;
            
            float u = (float)Heating.atmoViscAtRef0C * (Mathf.Pow((T / 273.15f), 0.7f));
            if(cell.Index == 10)
            {
                //Debug.Log("viscosity is: " + u);
            }
            
            return u;
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
