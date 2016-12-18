using UnityEngine;
using System.Collections;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class Constraint
    {
        // general for any constraint for Projective Dynamics
        public int[] indices { get; protected set; }
        public float w { get; protected set; }
        public Matrix<float> leftMatrix { get; protected set; }
        public Vector<float> projection { get; protected set; }


        public virtual void SetLeftMatrix(Matrix<float> matrix) { }

        public virtual void Project(ref Vector<float> x) { }
    }
}