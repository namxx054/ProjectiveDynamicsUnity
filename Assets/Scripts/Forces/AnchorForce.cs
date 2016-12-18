using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace PhysicallyBasedAnimations
{
    public class AnchorForce : MonoBehaviour
    {
        public int idx; // particle index
        public Vector<float> xFrom, vFrom; // point to anchor it to
        public Vector<float> xTo; // point to anchor it to
        public float ks = 5f, kd = 0.05f; // spring constant, damping coefficient

        private Vector<float> dx;

        void Start()
        {
            this.xFrom = Vector<float>.Build.Dense(3);
            this.vFrom = Vector<float>.Build.Dense(3);
            this.xTo = Vector<float>.Build.Dense(3);
            this.dx = Vector<float>.Build.Dense(3);
        }

        public void SetAnchorToPoint(Vector3 pos)
        {
            this.xTo[0] = pos[0];
            this.xTo[1] = pos[1];
            this.xTo[2] = pos[2];
        }

        public void AddForces(ref Vector<float> x, ref Vector<float> v, ref Vector<float> f)
        {
            if (this.idx < 0)
            {
                return;
            }

            Vector<float> xFrome = x.SubVector(this.idx * 3, 3);
            Vector<float> vFrome = v.SubVector(this.idx * 3, 3);

            this.dx = this.xFrom - this.xTo;

            f.SetSubVector(this.idx * 3, 3, f.SubVector(this.idx * 3, 3) + ((-ks * dx) - (kd * vFrom)));
        }
    }
}