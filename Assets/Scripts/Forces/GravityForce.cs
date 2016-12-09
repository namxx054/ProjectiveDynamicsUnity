using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace PhysicallyBasedAnimations
{
    public class GravityForce : Force
    {
        private ParticleSystem ps; // apply gravity to all particles
        private Vector<float> g;   // acceleration vector

        public void Init(ParticleSystem ps, Vector3 g)
        {
            this.ps = ps;
            this.g = Vector<float>.Build.Dense(3);
        }

        public void SetGravity(Vector3 g)
        {
            this.g[0] = g[0];
            this.g[1] = g[1];
            this.g[2] = g[2];
        }

        public override void AddForces(Vector<float> f)
        {
            for (int p = 0; p < this.ps._particles.Count; p++)
            {
                ParticleSystem.Particle particle = this.ps._particles[p];
                f.SetSubVector(p * 3, 3, f.SubVector(p * 3, 3) + (g * particle.m));
            }
        }
    }
}