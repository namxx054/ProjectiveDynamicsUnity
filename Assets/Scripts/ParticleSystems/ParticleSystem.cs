using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class ParticleSystem : PhysicalSystem
    {
        private float _t;
        public List<Particle> _particles;
        private List<Force> _forces;
        public float _msu; // coefficient of friction

        public override int GetNumDOFs()
        {
            return 3 * this._particles.Count;
        }

        public override void GetState(ref Vector<float> x, ref Vector<float> v, ref float t)
        {
            t = this._t;
            for (int p = 0; p < this._particles.Count; p++)
            {
                Particle particle = this._particles[p];
                x.SetSubVector(p * 3, 3, particle.x);
                v.SetSubVector(p * 3, 3, particle.v);
            }
        }

        public override void SetState(Vector<float> x, Vector<float> v, float t)
        {
            this._t = t;
            for (int p = 0; p < this._particles.Count; p++)
            {
                Particle particle = this._particles[p];
                particle.x = x.SubVector(p * 3, 3);
                particle.v = v.SubVector(p * 3, 3);
            }
        }

        public override void GetInertia(ref Matrix<float> M)
        {
            M.Clear();

            for (int p = 0; p < this._particles.Count; p++)
            {
                M.SetSubMatrix(p * 3, p * 3, 3, 3, this._particles[p].m * Matrix<float>.Build.DiagonalIdentity(3, 3));
            }
        }

        public override void GetForces(ref Vector<float> f)
        {
            for (int i = 0; i < this._particles.Count; i++)
            {
                f.SetSubVector(i * 3, 3, this._particles[i].f_ext);
            }

            for (int i = 0; i < this._forces.Count; i++)
            {
                // TODO
                // this._forces[i]->addForces(f);
            }
        }

        public override void GetAccelerations(ref Vector<float> a)
        {
            this.GetForces(ref a);
            for (int p = 0; p < this._particles.Count; p++)
            {
                a.SetSubVector(p * 3, 3, a.SubVector(p * 3, 3) / this._particles[p].m);
            }
        }

        public override void GetJacobians(ref Matrix<float> Jx, ref Matrix<float> Jv)
        {
            Jx.Clear();
            Jv.Clear();
            for (int i = 0; i < this._forces.Count; i++)
            {
                // TODO
                //this._forces[i]->addJacobians(Jx, Jv);
            }
        }

        public void ClearForces()
        {
            for (int i = 0; i < this._particles.Count; i++)
            {
                this._particles[i].f_ext[0] = 0f;
                this._particles[i].f_ext[1] = 0f;
                this._particles[i].f_ext[2] = 0f;
            }
        }

        public class Particle
        {
            public int i;          // index
            public float m;       // mass
            public Vector<float> x;     // position
            public Vector<float> v;     // velocity
            public Vector<float> f_ext; // external forces

            public Particle(int i, float m, Vector<float> x, Vector<float> v)
            {
                this.i = i;
                this.m = m;
                this.x = x;
                this.v = v;
                this.f_ext = Vector<float>.Build.Dense(3);
            }
        }

    }
}