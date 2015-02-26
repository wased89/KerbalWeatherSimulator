using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using UnityEngine;
using GeodesicGrid;

namespace Weather
{
    public class PlanetSimulator
    {
        private GameObject renderObject;
        public List<CellMap<WeatherCell>> LiveMap = new List<CellMap<WeatherCell>>();
        public List<CellMap<WeatherCell>> BufferMap = new List<CellMap<WeatherCell>>();
        private Vector3[] directionVertex;
        private Vector3 sunDir;
        
        public Func<Vector3> sunCallback;
        
        public Func<int, Cell, float> sunAngleCallback;

        public event Action bufferFlip;

        public float bodyKSun = 1366f; //default for earth
        public float geeASL = 9.81f; //default for earth
        public float MMOA = 0.028964f; //default for earth

        private int CellsToUpdate = 500;
        private int currentIndex;


        public int level
        {
            
            get;
            private set;
        }

        public PlanetSimulator(int gridLevel, int Layers, Func<Vector3> sunCallback)
        {
            //Callbacks are delegates that gives you shit, like Func<Vector3>
            //will match a function with return of vector3, useful shit btw.
            this.sunCallback = sunCallback;
            sunDir = sunCallback();
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
                foreach (Cell cell in Cell.AtLevel(gridLevel))
                {
                    
                    WeatherCell temp = new WeatherCell();
                    temp = WeatherCell.GetDefaultWeatherCell();
                    buffer[cell] = temp;

                }
                LiveMap.Add(buffer);
                BufferMap.Add(buffer);
                //Debug.Log("Finished adding layer: " + AltLayer);
            }
            LateInit();
        }

        void LateInit()
        {
            foreach(Cell cell in Cell.AtLevel(level))
            {
                Heating.initShortwaves(this, cell);
            }
        }

        public void SetSunAngleFunction(Func<int,Cell,float> func)
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
                    BufferMap[AltLayer][cell] = UpdateWeatherCell(AltLayer, cell, temp);
                }
                currentIndex++;
            }

            
            if (currentIndex >= (int)Cell.CountAtLevel(level)-1)
            {
                //Don't Worry, it makes sense. Trust me.
                Debug.Log("Resetting Index!");
                List<CellMap<WeatherCell>> temp = LiveMap;
                LiveMap = BufferMap;
                BufferMap = temp;

                currentIndex = 0;

                sunDir = sunCallback();
                bufferFlip();

            }

        }

        public WeatherCell UpdateWeatherCell(int AltLayer, Cell cell, WeatherCell wcell)
        {
            float TLR;
            if(AltLayer + 1 > LiveMap.Count-1)
            {
                TLR = -(2.7f - LiveMap[AltLayer][cell].Temperature);
                
            }
            else
            {
               TLR = -(LiveMap[AltLayer + 1][cell].Temperature - LiveMap[AltLayer][cell].Temperature);
                
            }

            wcell.Temperature = sunAngleCallback(AltLayer, cell);

            if (AltLayer + 1 > LiveMap.Count-1)
            {
                wcell.Pressure = WeatherFunctions.calculatePressure(wcell.Pressure, TLR, wcell.Temperature, wcell.Altitude,
               ((wcell.Altitude + 2500) - wcell.Altitude), geeASL, MMOA);

                wcell.Density = WeatherFunctions.calculateDensity(wcell.Density, TLR, wcell.Temperature, wcell.Altitude,
                    ((wcell.Altitude + 2500) - wcell.Altitude), geeASL, MMOA);
            }
            else
            {
                wcell.Pressure = WeatherFunctions.calculatePressure(wcell.Pressure, TLR, wcell.Temperature, wcell.Altitude,
                ((LiveMap[AltLayer +1][cell].Altitude) - wcell.Altitude), geeASL, MMOA);

                wcell.Density = WeatherFunctions.calculateDensity(wcell.Density, TLR, wcell.Temperature, wcell.Altitude,
                    ((LiveMap[AltLayer+1][cell].Altitude) - wcell.Altitude), geeASL, MMOA);
            }
            

            return wcell;
        }

    }
}
