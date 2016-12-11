using UnityEngine;
using System.Collections;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class SymplecticEuler : TimeIntegrator
    {
        private ParticleSystem ps;
        private float t;
        private Vector<float> x, v, a;

        public void Init(ParticleSystem system)
        {
            this.ps = system;
            int n = system.GetNumDOFs();
            // preallocate memeory
            this.x = Vector<float>.Build.Dense(n);
            this.v = Vector<float>.Build.Dense(n);
            this.a = Vector<float>.Build.Dense(n);
        }

        public override void step(float dt)
        {
            this.ps.GetState(x, v, ref t);
            this.ps.GetAccelerations(a);
            v += a * dt;
            x += v * dt;
            t += dt;
            this.ps.SetState(x, v, (t + dt));
        }
    }
}