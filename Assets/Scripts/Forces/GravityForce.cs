using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace PhysicallyBasedAnimations
{
    public class GravityForce : Force
    {
        public ParticleSystem ps; // apply gravity to all particles
        public Vector<float> g;         // acceleration vector

        public override void AddForces(Vector<float> f)
        {
            for (int p = 0; p < this.ps._particles.Count; p++)
            {
                ParticleSystem.Particle particle = this.ps._particles[p];
                f.SetSubVector(p * 3, 3, f.SubVector(p * 3, 3) + (g * particle.m));
            }
        }

        public override void AddJacobians(Matrix<float> Jx, Matrix<float> Jv)
        {
            throw new NotImplementedException();
        }
    }
}