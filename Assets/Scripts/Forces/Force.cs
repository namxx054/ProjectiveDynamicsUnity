using UnityEngine;
using System.Collections;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public abstract class Force : MonoBehaviour
    {
        public abstract void AddForces(Vector<float> f);
        public abstract void AddJacobians(Matrix<float> Jx, Matrix<float> Jv);
    }
}