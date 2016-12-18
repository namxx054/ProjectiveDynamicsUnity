using UnityEngine;
using System.Collections;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class Force : MonoBehaviour
    {
        public virtual void AddForces(ref Vector<float> f) { }
    }
}