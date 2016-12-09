using UnityEngine;
using System.Collections;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class SymplecticEuler : TimeIntegrator
    {
        public ParticleSystem system;
        private float t;
        private Vector<float> x, v, a;

        public void Init(ParticleSystem system)
        {
            this.system = system;
            int n = system.GetNumDOFs();
            // preallocate memeory
            this.x = Vector<float>.Build.Dense(n);
            this.v = Vector<float>.Build.Dense(n);
            this.a = Vector<float>.Build.Dense(n);
        }

        public override void step(float dt)
        {
            this.system.GetState(x, v, ref t);
            this.system.GetAccelerations(a);
            v += a * dt;
            x += v * dt;
            t += dt;
            this.system.SetState(x, v, (t + dt));
        }
    }
}