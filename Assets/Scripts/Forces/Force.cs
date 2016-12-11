using UnityEngine;
using System.Collections;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class Force : MonoBehaviour
    {
        public virtual void AddForces(ParticleSystem ps, Vector<float> f) { }
        public virtual void AddJacobians(ParticleSystem ps, Matrix<float> Jx, Matrix<float> Jv) { }
    }
}