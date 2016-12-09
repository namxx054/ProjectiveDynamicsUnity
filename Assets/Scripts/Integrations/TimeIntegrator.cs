using UnityEngine;
using System.Collections;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class TimeIntegrator : MonoBehaviour
    {
        public virtual void step(float dt) { }
    }
}