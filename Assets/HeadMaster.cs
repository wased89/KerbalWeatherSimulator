using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using KerbalWeatherSimulator;
using GUIUtils;
using GeodesicGrid;
using ObjectSim;

namespace KerbalWeatherSimulator
{
    public class HeadMaster : MonoBehaviour
    {
        PlanetSimulator pSim;
        SimulatorDisplay simDisplay;
        SunMove sunMove;
        //CloudSystem cloudSystem;
        AxisRenderer aRenderer;
        DebugGUI debug;
        

        private GameObject mainCamera;
        private float cameraLat = Mathf.PI / 2f;
        private float cameraLong = 0f;
        private float cameraDistance = 3f;
        public static  float sunRotationSpeed = 0f;
        public static bool test = true;
        private Vector3 angularVelocity = new Vector3(0,2f,0);

        void Awake()
        {
            sunMove = new SunMove();
            pSim = new PlanetSimulator(5, 5, SunFunction, sunAngle, angularVelocity);
            simDisplay = new SimulatorDisplay(pSim, DisplayMapType.PRESSURE_MAP);
            aRenderer = new AxisRenderer();
            debug = new DebugGUI(pSim);
            //cloudSystem = new CloudSystem(pSim);
            
            pSim.bufferFlip += simDisplay.OnBufferChange;


            if (mainCamera == null)
            {
                
                mainCamera = GameObject.Find("Main Camera");
                SetCamera();
                
            }
        }

        void Update()
        {
            HandleKeyInput();
            pSim.Update();
            sunMove.Update();
            simDisplay.Update();
            debug.Update();
            //cloudSystem.UpdateParticleCells();
            
        }

        void FixedUpdate()
        {

        }

        void OnGUI()
        {
            debug.OnGUI();
        }
        void Draw()
        {

        }
        void HandleKeyInput()
        {
            bool setCam = false;
            if (Input.GetKey(KeyCode.A))
            {
                setCam = true;
                cameraLong -= Time.deltaTime;
                if (cameraLong < 0f)
                {
                    cameraLong = Mathf.PI * 2f;
                }
            }
            if (Input.GetKey(KeyCode.D))
            {
                setCam = true;
                cameraLong += Time.deltaTime;
                if (cameraLong > Mathf.PI * 2f)
                {
                    cameraLong = 0f;
                }
            }
            if (Input.GetKey(KeyCode.W))
            {
                setCam = true;
                cameraLat -= Time.deltaTime;
                if (cameraLat < 0f)
                {
                    cameraLat = 0f;
                }
            }
            if (Input.GetKey(KeyCode.S))
            {
                setCam = true;
                cameraLat += Time.deltaTime;
                if (cameraLat > Mathf.PI)
                {
                    cameraLat = Mathf.PI;
                }
            }
            if (Input.GetKey(KeyCode.X))
            {
                setCam = true;
                cameraDistance -= Time.deltaTime;
                if (cameraDistance < 0.1f)
                {
                    cameraDistance = 0.1f;
                }
            }
            if (Input.GetKey(KeyCode.Z))
            {
                setCam = true;
                cameraDistance += Time.deltaTime;
                if (cameraDistance > 10f)
                {
                    cameraDistance = 10f;
                }
            }
            if(Input.GetKey(KeyCode.E))
            {
                setCam = true;
                sunRotationSpeed += 0.01f;

            }
            if (Input.GetKey(KeyCode.Q))
            {
                setCam = true;
                sunRotationSpeed -= 0.01f;
                if(sunRotationSpeed <= 0f)
                {
                    sunRotationSpeed = 0f;
                }

            }
            //SetCamera();
            if (setCam)
            {
                SetCamera();
            }
        }
        void SetCamera()
        {
            
            float newX = cameraDistance * (Mathf.Sin(cameraLat) * Mathf.Cos(cameraLong));
            float newY = cameraDistance * Mathf.Cos(cameraLat);
            float newZ = cameraDistance * (Mathf.Sin(cameraLat) * Mathf.Sin(cameraLong));
            Vector3 newPos = new Vector3(newX, newY, newZ);
            mainCamera.camera.transform.position = newPos;
            mainCamera.camera.transform.rotation = Quaternion.LookRotation(-mainCamera.camera.transform.position.normalized);
        }

        public Vector3 SunFunction()
        {
            GameObject sun = GameObject.Find("Sun");
            return sun.transform.position.normalized;
            //return new Vector3(x , y, z );
        }
        public float sunAngle(Vector3 sunDir, int AltLayer, Cell cell)
        {

            return Mathf.Max(Vector3.Dot(cell.Position, sunDir),0);

        }


        //Test code for coroutine setup

        void testStart()
        {
            StartCoroutine(StepThroughCoroutine(UpdateTheCells(5)));
        }

        IEnumerator StepThroughCoroutine(IEnumerator crStepThrough)
        {
            if(Input.GetKeyDown(KeyCode.G) && crStepThrough.MoveNext())
            {
                yield return crStepThrough.Current;
            }
            else
            {
                yield return null;
            }
        }

        IEnumerator UpdateTheCells(int gridLevel)
        {
            foreach(Cell cell in Cell.AtLevel(gridLevel))
            {
                yield return UpdateTheCell(cell);
            }
            
        }

        IEnumerator UpdateTheCell(Cell cell)
        {
            Debug.Log("Cell!");
            yield return cell;
        }


    }
}
