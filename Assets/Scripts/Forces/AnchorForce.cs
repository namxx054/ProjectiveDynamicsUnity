using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace PhysicallyBasedAnimations
{
    public class AnchorForce : Force
    {
        public ParticleSystem.Particle p; // particle
        public Vector<float> x; // point to anchor it to
        public float ks = 5f, kd = 0.05f; // spring constant, damping coefficient

        private Vector<float> dx;

        void Start()
        {
            this.x = Vector<float>.Build.Dense(3);
            this.dx = Vector<float>.Build.Dense(3);
        }
        
        public override void AddForces(ParticleSystem ps, Vector<float> f)
        {
            if (this.p == null)
            {
                return;
            }

            this.dx = this.p.x - this.x;
            f.SetSubVector(p.i * 3, 3, f.SubVector(p.i * 3, 3) + ((-ks * dx) - (kd * p.v)));
        }
    }
}