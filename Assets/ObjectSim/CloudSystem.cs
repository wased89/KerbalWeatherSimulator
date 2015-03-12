using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalWeatherSimulator;
using GeodesicGrid;
using UnityEditor;

namespace ObjectSim
{
    class CloudSystem
    {
        GameObject urf;
        ParticleEmitter pEmitter = new ParticleEmitter();
        ParticleRenderer pRenderer = new ParticleRenderer();
        ParticleAnimator pAnimator = new ParticleAnimator();
        Particle[] particles;
        PlanetSimulator pSim;
        private Dictionary<Cell, Particle> particleCellMap = new Dictionary<Cell, Particle>();

        public CloudSystem(PlanetSimulator pSim)
        {
            this.pSim = pSim;
            urf = GameObject.Find("Earth");
            
            pEmitter = (ParticleEmitter)urf.AddComponent("MeshParticleEmitter");
            pRenderer = urf.AddComponent<ParticleRenderer>();
            pAnimator = urf.AddComponent<ParticleAnimator>();

            pEmitter.useWorldSpace = false;
            
            pEmitter.maxEmission = 5000f;
            

            particles = new Particle[Cell.CountAtLevel(pSim.level)];

            //pEmitter.particles = particles;
            
            initParticleMap();
        }

        void initParticleMap()
        {
            Debug.Log("Initing Particle Maps");

            //ParticleEmitter pEmitter = urf.particleEmitter;

            foreach(Cell cell in Cell.AtLevel(pSim.level))
            {

                Particle p = new Particle();
                p.position = new Vector3(cell.Position.x * 2.2f, cell.Position.y * 2.2f, cell.Position.z * 2.2f);
                p.size = 0.1f;
                p.velocity = Vector3.zero;
                particleCellMap.Add(cell,p);
                //pEmitter.particles[0] = p;
                
                particles[cell.Index] = p;
                
                pEmitter.Emit(p.position,p.velocity,p.size,p.energy,p.color);
            }
            particles = particleCellMap.Values.ToArray<Particle>();
            
            //pEmitter.particles = particles;
            Texture2D text = new Texture2D(1,1);
            pRenderer.enabled = true;
            pRenderer.material.mainTexture = text;
            text.wrapMode = TextureWrapMode.Clamp;
            //pRenderer.Render(1);
        }
        public void UpdateParticleCells()
        {
            
            foreach(Cell cell in Cell.AtLevel(pSim.level))
            {
                var temp = particleCellMap;
                Particle p = temp[cell];
                //pAnimator.force = pSim.LiveMap[0][cell].WindDirection.normalized;
                
                p.velocity = pSim.LiveMap[0][cell].WindDirection.normalized;
                particles[cell.Index] = p;
            }
            //pSystem.SetParticles(particles, particles.Length);
            //pEmitter.particles = particles;
            
           
        }
        
    }
}
