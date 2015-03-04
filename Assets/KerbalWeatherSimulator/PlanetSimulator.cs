using System;
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
        //fdasfdafdas
        public List<CellMap<WeatherCell>> LiveMap = new List<CellMap<WeatherCell>>();
        public List<CellMap<WeatherCell>> BufferMap = new List<CellMap<WeatherCell>>();
        

        public Vector3 sunDir
        {
            private set;
            get;
        }
        
        private Func<Vector3> sunCallback;
        
        private Func<Vector3, int, Cell, float> sunAngleCallback;

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

        public PlanetSimulator(int gridLevel, int Layers, Func<Vector3> sunDirCallback, Func<Vector3, int, Cell, float> sunAngleCallback)
        {
            //Callbacks are delegates that gives you shit, like Func<Vector3>
            //will match a function with return of vector3, useful shit btw.
            this.sunCallback = sunDirCallback;
            this.sunAngleCallback = sunAngleCallback;
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
                    temp.Pressure = GenerateRandomPressure(cell);
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
            foreach(Cell cell in Cell.AtLevel(level))
            {
                //Heating.initShortwaves(this, cell);
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

        public void CalculateCoriolisAcc()
        {
            //ac = -2(angularVelocity vector) X v
            //X = cross, v = velocity of object
            //ac = -2 * Vector3.Cross(angularVelocity, v)
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
                //Debug.Log("Resetting Index!");
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

            wcell.Temperature = sunAngleCallback(sunDir, AltLayer, cell);

            if (AltLayer + 1 > LiveMap.Count-1)
            {
                //wcell.Pressure = WeatherFunctions.calculatePressure(wcell.Pressure, TLR, wcell.Temperature, wcell.Altitude,
               //((wcell.Altitude + 2500) - wcell.Altitude), geeASL, MMOA);

                //wcell.Density = WeatherFunctions.calculateDensity(wcell.Density, TLR, wcell.Temperature, wcell.Altitude,
                    //((wcell.Altitude + 2500) - wcell.Altitude), geeASL, MMOA);
            }
            else
            {
                //wcell.Pressure = WeatherFunctions.calculatePressure(wcell.Pressure, TLR, wcell.Temperature, wcell.Altitude,
                //((LiveMap[AltLayer +1][cell].Altitude) - wcell.Altitude), geeASL, MMOA);

                //wcell.Density = WeatherFunctions.calculateDensity(wcell.Density, TLR, wcell.Temperature, wcell.Altitude,
                    //((LiveMap[AltLayer+1][cell].Altitude) - wcell.Altitude), geeASL, MMOA);
            }
            wcell.Pressure = RandomPressure(0, cell);
            wcell.WindDirection = WeatherFunctions.CalculateWindVector(this, AltLayer, cell);

            return wcell;
        }

    }
}
