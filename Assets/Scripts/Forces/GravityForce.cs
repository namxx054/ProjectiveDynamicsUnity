using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace PhysicallyBasedAnimations
{
    public class GravityForce : Force
    {
        public Vector3 dir = new Vector3(0, -9.8f, 0);

        private Vector<float> g;

        void Start()
        {
            this.g = Vector<float>.Build.Dense(3);
        }

        public void SetGravity(Vector3 dir)
        {
            this.g[0] = dir[0];
            this.g[1] = dir[1];
            this.g[2] = dir[2];
        }

        public override void AddForces(ParticleSystem ps, Vector<float> f)
        {
            // update "g" from "dir"
            this.SetGravity(this.dir); // for testing in the editor with Vector3

            for (int p = 0; p < ps.particles.Count; p++)
            {
                ParticleSystem.Particle particle = ps.particles[p];
                f.SetSubVector(p * 3, 3, f.SubVector(p * 3, 3) + (g * particle.m));
            }
        }
    }
}