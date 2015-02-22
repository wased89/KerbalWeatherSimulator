﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Weather
{
    public struct WeatherCell
    {
        //Private Variables


        //Public Variables

        //Weather States
        //public byte WEATHERSTATE;
        public bool Clouded;

        //Day states
        //public bool Daytime;

        //Miscellaneous
        //public bool isCellHigherPressure;
        //public bool hasThermal;

        //Scanning bools
        //public bool isScanning;
        public bool isOcean;

        //Weather Stats
        

        public double Pressure;
        public double Density;

        //Radiation
        public float SWReflected;
        public float SWAbsorbed;
        public float SWTransmitted;

        public float LWIn;
        public float LWOut;
        public float LWTransmit;

        public float Transmissivity;
        public float Emissivity;
        public float Albedo;

        public float Humidity;
        public float Temperature;
        public float Altitude;
        //public float WindSpeed;

        public Vector3 WindDirection;


        //public Color32 cloudColour;

        public static WeatherCell GetDefaultWeatherCell()
        {
            WeatherCell cell;
            //cell.WEATHERSTATE = 0;
            //cell.isScanning = false;
            //cell.hasThermal = false;
            cell.isOcean = false;
            cell.Clouded = false;
            //cell.Daytime = true;
            //cell.isCellHigherPressure = false;
            cell.Albedo = 0.35f;
            cell.Altitude = 0;
            cell.Density = 0;
            cell.Humidity = 0;
            cell.Temperature = 0;
            cell.WindDirection = Vector3.zero;
            //cell.WindSpeed = 0;
            cell.Pressure = 1;
            cell.Transmissivity = 0;
            cell.Emissivity = 0;
            cell.SWAbsorbed = 0;
            cell.SWReflected = 0;
            cell.SWTransmitted = 0;
            cell.LWIn = 1;
            cell.LWOut = 1;
            cell.LWTransmit = 1;
            //cloudColour = new Color32(1,1,1,1);
            return cell;
        }
    }
}
