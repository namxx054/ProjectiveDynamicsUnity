using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class ParticleSystem : PhysicalSystem
    {
        public List<Particle> particles { get; private set; }
        public List<Force> forces = new List<Force>();
        private float m_t;
        
        public void Init(TetMesh mesh, float pMass)
        {
            this.particles = new List<Particle>();
            this.m_t = 0f;

            //
            for (int i = 0; i < mesh.vertices_init.Count; i++)
            {
                Vector<float> x = Vector<float>.Build.DenseOfArray(new float[] { mesh.vertices_init[i][0], mesh.vertices_init[i][1], mesh.vertices_init[i][2] });
                Vector<float> v = Vector<float>.Build.Dense(3);

                Particle p = new Particle(i, pMass, x, v);
                this.particles.Add(p);
           } 
        }


        public override int GetNumDOFs()
        {
            return 3 * this.particles.Count;
        }


        public override void GetState(Vector<float> x, Vector<float> v, ref float t)
        {
            t = this.m_t;
            for (int p = 0; p < this.particles.Count; p++)
            {
                Particle particle = this.particles[p];
                x.SetSubVector(p * 3, 3, particle.x);
                v.SetSubVector(p * 3, 3, particle.v);
            }
        }


        public override void SetState(Vector<float> x, Vector<float> v, float t)
        {
            this.m_t = t;
            for (int p = 0; p < this.particles.Count; p++)
            {
                Particle particle = this.particles[p];
                particle.x = x.SubVector(p * 3, 3);
                particle.v = v.SubVector(p * 3, 3);
            }
        }


        public override void GetInertia(Matrix<float> M)
        {
            M.Clear();

            for (int p = 0; p < this.particles.Count; p++)
            {
                M.SetSubMatrix(p * 3, p * 3, 3, 3, this.particles[p].m * Matrix<float>.Build.DiagonalIdentity(3, 3));
            }
        }


        public override void GetForces(Vector<float> f)
        {
            for (int i = 0; i < this.particles.Count; i++)
            {
                f.SetSubVector(i * 3, 3, this.particles[i].f_ext);
            }
            for (int i = 0; i < this.forces.Count; i++)
            {
                this.forces[i].AddForces(ref f);
            }
        }


        public override void GetAccelerations(Vector<float> a)
        {
            this.GetForces(a);

            for (int p = 0; p < this.particles.Count; p++)
            {
                a.SetSubVector(p * 3, 3, a.SubVector(p * 3, 3) / this.particles[p].m);
            }
        }

        
        public void ClearParticleForces()
        {
            for (int i = 0; i < this.particles.Count; i++)
            {
                this.particles[i].f_ext[0] = 0f;
                this.particles[i].f_ext[1] = 0f;
                this.particles[i].f_ext[2] = 0f;
            }
        }


        public Particle GetClosestParticle(Vector3 pos, float maxDist)
        {
            Particle p = null;
            float minDist = maxDist;
            for (int i = 0; i < this.particles.Count; i++)
            {
                float dist = Mathf.Sqrt((pos.x - this.particles[i].x[0]) * (pos.x - this.particles[i].x[0]) +
                    (pos.y - this.particles[i].x[1]) * (pos.x - this.particles[i].x[1]) +
                    (pos.z - this.particles[i].x[2]) * (pos.x - this.particles[i].x[2]));

                if (dist < minDist)
                {
                    minDist = dist;
                    p = this.particles[i];
                }
            }
            return p;
        }


        public class Particle
        {
            public int i;               // index
            public float m;             // mass
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