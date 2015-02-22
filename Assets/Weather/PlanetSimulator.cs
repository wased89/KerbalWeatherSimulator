﻿using System;
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
        public event Action bufferFlip;
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
                //Debug.Log("Finished adding layer: " + AltLayer);
            }
            
        }

        public void SetCellsToUpdate(int numb)
        {
            this.CellsToUpdate = numb;
        }

        public void Update()
        {
            UpdateNCells(CellsToUpdate);
        }

        public void UpdateNCells(int CellsToUpdate)
        {
            int cellsToProcess = (int)Cell.CountAtLevel(level) - currentIndex;
            if(cellsToProcess < CellsToUpdate)
            {
                CellsToUpdate = (int)Cell.CountAtLevel(level) - cellsToProcess;
            }

            for(int i = 0; i < CellsToUpdate; i ++)
            {
                for (int AltLayer = 0; AltLayer < LiveMap.Count; AltLayer++ )
                {
                    Cell cell = new Cell((uint)currentIndex);
                    WeatherCell temp = LiveMap[AltLayer][cell];

                    BufferMap[AltLayer][cell] = UpdateWeatherCell(cell, temp);
                }
                
                currentIndex++;
                
            }
            if (currentIndex == (int)Cell.CountAtLevel(level))
            {
                //Don't Worry, it makes sense. Trust me.
                List<CellMap<WeatherCell>> temp = LiveMap;
                LiveMap = BufferMap;
                BufferMap = temp;

                currentIndex = 0;

                sunDir = sunCallback();
                bufferFlip();

            }

        }

        public WeatherCell UpdateWeatherCell(Cell cell, WeatherCell wcell)
        {
            
            wcell.Pressure = 1.0;
            wcell.Temperature = (float)Math.Max(Vector3.Dot(cell.Position, sunDir), 0);

            return wcell;
        }

    }
}
