using UnityEngine;
using System.Collections;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class PhysicalSystem
    {

        // returns number of *positional* degrees of freedom (not velocity)
        public virtual int GetNumDOFs() { return 0; }

        // writes position, velocity, time into arguments
        public virtual void GetState(ref Vector<float> x, ref Vector<float> v, ref float t) { }

        // reads position, velocity, time from arguments
        public virtual void SetState(Vector<float> x, Vector<float> v, float t) { }

        // writes inertia matrix
        public virtual void GetInertia(ref Matrix<float> M) { }

        // writes forces
        public virtual void GetForces(ref Vector<float> f) { }

        // writes accelerations (should be the same as M^-1 f)
        public virtual void GetAccelerations(ref Vector<float> a) { }

        // writes Jacobians
        public virtual void GetJacobians(ref Matrix<float> Jx, ref Matrix<float> Jv) { }
    }
}