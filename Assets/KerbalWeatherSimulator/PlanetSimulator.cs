using System;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using UnityEngine;
using GeodesicGrid;
using Random = UnityEngine.Random;

namespace KerbalWeatherSimulator
{
    public class PlanetSimulator
    {
        //fdasfdsafdas
        
        public List<CellMap<WeatherCell>> LiveMap = new List<CellMap<WeatherCell>>();
        public List<CellMap<WeatherCell>> BufferMap = new List<CellMap<WeatherCell>>();
        

        public Vector3 sunDir
        {
            private set;
            get;
        }
        
        private Func<Vector3> sunCallback;
        
        private Func<Vector3, int, Cell, float> sunAngleCallback;

        public Vector3 angularVelocity;

        public event Action bufferFlip;

        public float bodyRadius = 6371000f; //default for earth
        public float bodyKSun = 1366f; //default for earth
        public float geeASL = 9.81f; //default for earth
        public float MMOA = 0.028964f; //default for earth

        private int CellsToUpdate = 250;
        private int currentIndex;


        public int level
        {
            
            get;
            private set;
        }

        public PlanetSimulator(int gridLevel, int Layers, Func<Vector3> sunDirCallback, Func<Vector3, int, Cell, float> sunAngleCallback, Vector3 angularVelocity)
        {
            //Callbacks are delegates that gives you shit, like Func<Vector3>
            //will match a function with return of vector3, useful shit btw.
            this.sunCallback = sunDirCallback;
            this.sunAngleCallback = sunAngleCallback;
            this.angularVelocity = angularVelocity;
            sunDir = sunDirCallback();
            Generate(gridLevel, Layers);
        }

        
        public void Generate(int gridLevel, int Layers)
        {
            level = gridLevel;
            LiveMap = new List<CellMap<WeatherCell>>();
            BufferMap = new List<CellMap<WeatherCell>>();

            //Debug.Log("Layers: " + Layers);

            for (int AltLayer = 0; AltLayer < Layers; AltLayer++ )
            {
                //Debug.Log("I'm currently on layer: " + AltLayer);
                CellMap<WeatherCell> buffer = new CellMap<WeatherCell>(gridLevel);
                CellMap<WeatherCell> buffer2 = new CellMap<WeatherCell>(gridLevel);
                foreach (Cell cell in Cell.AtLevel(gridLevel))
                {
                    
                    WeatherCell temp = new WeatherCell();
                    temp = WeatherCell.GetDefaultWeatherCell();
                    temp.Altitude = AltLayer * 2500;
                    int rnd = Random.Range(0,2);
                    if (rnd == 0) { temp.Clouded = false; }
                    else { temp.Clouded = true; }
                    temp.Albedo = Heating.calculateAlbedo(this, AltLayer, cell);
                    temp.Pressure = 101325f;
                    temp.Temperature = 19.45f + 273.15f;
                    
                    buffer[cell] = temp;
                    buffer2[cell] = temp;

                }
                LiveMap.Add(buffer);
                BufferMap.Add(buffer2);
                //Debug.Log("Finished adding layer: " + AltLayer);
            }
            LateInit();
        }

        void LateInit()
        {
            Debug.Log("LateInit!");

            
            for (int AltLayer = 0; AltLayer < LiveMap.Count; AltLayer++ )
            {
                foreach(Cell cell in Cell.AtLevel(level))
                {
                    WeatherCell temp = LiveMap[AltLayer][cell];

                    temp.Emissivity = Heating.calculateEmissivity(this, AltLayer, cell);
                    temp.Transmissivity = Heating.calculateTransmissivity(this, AltLayer, cell);
                    LiveMap[AltLayer][cell] = temp;
                    BufferMap[AltLayer][cell] = temp;
                }
            }

            foreach(Cell cell in Cell.AtLevel(level))
            {
                Heating.InitShortwaves(this, cell);
                Heating.InitLongwaves(this, cell);
            }
            
        }

        public void SetSunAngleFunction(Func<Vector3, int,Cell,float> func)
        {
            this.sunAngleCallback = func;
        }

        public void SetCellsToUpdate(int numb)
        {
            this.CellsToUpdate = numb;
        }
        public void SetMolarMassOfAir(float numb)
        {
            this.MMOA = numb;
        }

        public void SetBodyKSun(float numb)
        {
            this.bodyKSun = numb;
        }
        public float GenerateRandomPressure(Cell cell)
        {
            // pressure = 101325 + Math.Rand(0-50)
            
            return 101325f + ((Mathf.Sin(cell.Position.x * 10f) * Mathf.Sin(cell.Position.z * 10f)) * 5000f);
        }
        public float RandomPressure(int AltLayer, Cell cell)
        {
            return 101325f + Random.Range(-5000f, 5000f); //Random.Range(-5000f,5000f);
        }

        public float RandomTemperature(int AltLayer, Cell cell)
        {
            return 273.15f + (sunAngleCallback(sunDir,AltLayer, cell) * 150f);
        }

        public void SetInitTempOfCell(float temperature, int AltLayer, Cell cell)
        {
            WeatherCell wcell = LiveMap[AltLayer][cell];
            wcell.Temperature = temperature;
        }
        public void SetInitPressureOfCell(float pressure, int AltLayer, Cell cell)
        {
            WeatherCell wcell = LiveMap[AltLayer][cell];
            wcell.Pressure = pressure;
        }
        public void SetInitDensityOfCell(float density, int AltLayer, Cell cell)
        {
            WeatherCell wcell = LiveMap[AltLayer][cell];
            wcell.Density = density;
        }

        public void Update()
        {
            UpdateNCells(CellsToUpdate);
            
        }

        public void UpdateNCells(int CellsToUpdate)
        {
            
            CellsToUpdate = (int)Math.Min(Cell.CountAtLevel(level), currentIndex + CellsToUpdate);

            while(currentIndex < CellsToUpdate)
            {
                //Debug.Log(currentIndex + ", " + Cell.CountAtLevel(level));
                for (int AltLayer = 0; AltLayer < LiveMap.Count; AltLayer++)
                {
                    //Debug.Log("Currently Updating Cell: "+ currentIndex);

                    Cell cell = new Cell((uint)currentIndex);
                    WeatherCell temp = LiveMap[AltLayer][cell];
                    temp = UpdateWeatherCell(AltLayer, cell, BufferMap[AltLayer][cell]);
                    //BufferMap[AltLayer][cell] = UpdateWeatherCell(AltLayer, cell, temp);
                    BufferMap[AltLayer][cell] = temp;
                    
                    //BufferMap[AltLayer][cell] = UpdateWeatherCell(AltLayer, cell,LiveMap[AltLayer][cell]);
                    //LiveMap[AltLayer][cell] = UpdateWeatherCell(AltLayer, cell, BufferMap[AltLayer][cell]);
                    temp = BufferMap[AltLayer][cell];
                    LiveMap[AltLayer][cell] = UpdateWeatherCell(AltLayer, cell, temp);
                    //LiveMap[AltLayer][cell] = UpdateWeatherCell(AltLayer, cell, BufferMap[AltLayer][cell]);
                    //LiveMap[AltLayer][cell] = temp;
                    //LiveMap[AltLayer][cell] = BufferMap[AltLayer][cell];
                }
                currentIndex++;
            }

            
            if (currentIndex >= (int)Cell.CountAtLevel(level)-1)
            {
                //Don't Worry, it makes sense. Trust me.
                Debug.Log("Resetting Index!");
                /*
                List<CellMap<WeatherCell>> temp = LiveMap;
                LiveMap = BufferMap;
                BufferMap = temp;
                //*/
                //LiveMap = BufferMap;
                //BufferMap = LiveMap;
                currentIndex = 0;

                sunDir = sunCallback();
                bufferFlip();

            }

        }

        public WeatherCell UpdateWeatherCell(int AltLayer, Cell cell, WeatherCell wcell)
        {

            wcell.Temperature = Heating.CalculateTemperature(this, AltLayer, cell);

            float TLR;
            if(AltLayer + 1 > LiveMap.Count-1)
            {
                TLR = (2.7f - wcell.Temperature);
                
            }
            else
            {
               TLR = (LiveMap[AltLayer + 1][cell].Temperature - wcell.Temperature);
                
            }

            //wcell.Temperature = RandomTemperature(AltLayer, cell);
            //wcell.Temperature = 273.15f + sunAngleCallback(sunDir,AltLayer, cell);
            
            //wcell.Temperature = LiveMap[AltLayer][cell].Temperature + 1;
            
            //For pressure and density we need the base pressures for the layer... we need to get those...
            //the static of 101325 is going to fuck with calcs tbh.
            if (AltLayer + 1 > LiveMap.Count-1)
            {
                //wcell.Pressure = WeatherFunctions.calculatePressure(LiveMap[AltLayer][cell].Pressure, TLR, wcell.Temperature, wcell.Altitude,
                    //((wcell.Altitude + 2500) - wcell.Altitude), geeASL, MMOA);

                //wcell.Density = WeatherFunctions.calculateDensity(1.235f, TLR, wcell.Temperature, wcell.Altitude,
                    //((wcell.Altitude + 2500) - wcell.Altitude), geeASL, MMOA);
            }
            else
            {
                //wcell.Pressure = WeatherFunctions.calculatePressure(LiveMap[AltLayer][cell].Pressure, TLR, wcell.Temperature, wcell.Altitude,
                    //((LiveMap[AltLayer +1][cell].Altitude) - wcell.Altitude), geeASL, MMOA);

                //wcell.Density = WeatherFunctions.calculateDensity(1.235f, TLR, wcell.Temperature, wcell.Altitude,
                    //((LiveMap[AltLayer+1][cell].Altitude) - wcell.Altitude), geeASL, MMOA);
            }
            //wcell.Pressure = GenerateRandomPressure(cell);
            wcell.Pressure = WeatherFunctions.newCalculatePressure(this, AltLayer, cell);
            
            wcell.WindDirection = WeatherFunctions.CalculateWindVector(this, AltLayer, cell);

            //if (cell.Index == 10 && AltLayer == 0) { Debug.Log("Albedo: " + wcell.Albedo); }
            //if (cell.Index == 10 && AltLayer == 0) { Debug.Log("Temperature: " + wcell.Temperature); }
            //if (cell.Index == 10 && AltLayer == 0) { Debug.Log("Pressure: " + wcell.Pressure); }
            //if (cell.Index == 10 && AltLayer == 0) { Debug.Log("Density: " + wcell.Density); }
            //if (cell.Index == 10 && AltLayer == 0) { Debug.Log("Emissivity: " + wcell.Emissivity); }
            //if (cell.Index == 10 && AltLayer == 0) { Debug.Log("SunAngle: " + Heating.getSunlightAngle(this, AltLayer, cell)); }

            return wcell;
        }

    }

}
