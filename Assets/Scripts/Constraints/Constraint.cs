using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class Constraint
    {
        public virtual void Project(ref Vector<float> x) { }
    }
}