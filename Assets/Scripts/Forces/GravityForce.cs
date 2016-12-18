using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace PhysicallyBasedAnimations
{
    public class GravityForce : MonoBehaviour
    {
        public Vector3 dir = new Vector3(0, -9.8f, 0);

        public void AddForces(ref Vector<float> f)
        {
            for (int i = 0; i < f.Count; i += 3)
            {
                f[i + 0] += this.dir[0];
                f[i + 1] += this.dir[1];
                f[i + 2] += this.dir[2];
            }
        }
    }
}