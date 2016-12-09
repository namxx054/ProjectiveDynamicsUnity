using UnityEngine;
using System.Collections;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class PhysicalSystem : MonoBehaviour
    {
        // returns number of *positional* degrees of freedom (not velocity)
        public virtual int GetNumDOFs() { return 0; }

        // writes position, velocity, time into arguments
        public virtual void GetState(Vector<float> x, Vector<float> v, ref float t) { }

        // reads position, velocity, time from arguments
        public virtual void SetState(Vector<float> x, Vector<float> v, float t) { }

        // writes inertia matrix
        public virtual void GetInertia(Matrix<float> M) { }

        // writes forces
        public virtual void GetForces(Vector<float> f) { }

        // writes accelerations (should be the same as M^-1 f)
        public virtual void GetAccelerations(Vector<float> a) { }

        // writes Jacobians
        public virtual void GetJacobians(Matrix<float> Jx, Matrix<float> Jv) { }
    }
}