using UnityEngine;
using System.Collections;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class Force : MonoBehaviour
    {
        public virtual void AddForces(Vector<float> f) { }
        public virtual void AddJacobians(Matrix<float> Jx, Matrix<float> Jv) { }
    }
}