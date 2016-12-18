using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace PhysicallyBasedAnimations
{
    public class ProjectiveDynamics : MonoBehaviour
    {
        public int iterationNumber = 1;

        public Vector2 strainRange;

        private TetMesh mesh;

        public TetMesh testMesh;

        public GravityForce f_gravity;
        public AnchorForce f_anchor;

        public Vector<float> q_n, v_n, f_n; // current states
        public Vector<float> q_n1, v_n1; // next states
        private Matrix<float> M; // assume fixed
        private Matrix<float> M_inverse; // assume fixed
        private float h;
        private float t;

        private Matrix<float> leftMatrix;
        private Cholesky<float> cholesky;
        private Vector<float> rightVector;

        public List<Constraint> constraints = new List<Constraint>();

        public void Init(TetMesh mesh, float pMass, float h)
        {
            this.mesh = mesh;
            mesh.Init();

            if (testMesh != null)
            {
                this.testMesh.filename = mesh.filename;
                this.testMesh.Init();
            }

            // 
            int n = mesh.vertices_init.Count * 3;
            Debug.Log("# of DOF: " + n);
            this.q_n = Vector<float>.Build.Dense(n);
            this.v_n = Vector<float>.Build.Dense(n);
            this.f_n = Vector<float>.Build.Dense(n);
            this.M = Matrix<float>.Build.SparseDiagonal(n, n, pMass);
            this.M_inverse = M.Inverse();
            this.h = h;
            //
            this.q_n1 = Vector<float>.Build.Dense(n);
            this.v_n1 = Vector<float>.Build.Dense(n);
            //
            this.SetFromTetMesh(this.mesh, ref this.q_n);
            this.CreateTetConstraints();
            Debug.Log("- created constraints(s): " + this.constraints.Count);
            this.leftMatrix = Matrix<float>.Build.Dense(n, n);
            this.rightVector = Vector<float>.Build.Dense(n);
            //
            this.UpdateLeftMatrix();
            this.UpdateConstraintLeftMatrix();
        }


        public Vector3 GetPosOfParticleAt(int i)
        {
            Vector<float> vec = this.q_n.SubVector(i * 3, 3);
            return new Vector3(vec[0], vec[1], vec[2]);
        }

        public void step()
        {
            //
            this.f_n.Clear();
            this.f_gravity.AddForces(ref this.f_n);
            if (this.f_anchor.idx > 0)
            {
                this.f_anchor.AddForces(ref this.q_n, ref this.v_n, ref this.f_n);
            }

            // 1. compute s_n (momentum)
            Vector<float> s_n = this.q_n + (this.v_n * h) + (h * h * M_inverse * f_n);
            s_n.CopyTo(this.q_n1);

            // 2. M/(h^2)
            s_n = (M / (h * h)) * s_n;

            //
            for (int iter = 0; iter < this.iterationNumber; iter++)
            {
                this.rightVector = s_n;

                // 3. Local Step
                for (int c = 0; c < this.constraints.Count; c++)
                {
                    TetConstraint currConstraint = (TetConstraint)this.constraints[c];
                    currConstraint.Project(ref q_n1);
                    this.rightVector += (currConstraint.leftMatrix * currConstraint.projection);
                }

                // 4. Global Step => solve for q_n1
                // this.q_n1 = this.leftMatrix.Solve(this.rightVector);
                this.q_n1 = this.cholesky.Solve(this.rightVector);
            }

            // 5. Update the state.
            this.v_n1 = (q_n1 - q_n) / h;

            this.q_n = this.q_n1;
            this.v_n = this.v_n1;
        }


        private void UpdateLeftMatrix()
        {
            this.leftMatrix.Clear();

            Matrix<float> A = this.CreateAMatrix();
            Matrix<float> A_transpose = A.Transpose();

            this.leftMatrix += (this.M / (h * h));

            for (int i = 0; i < this.constraints.Count; i++)
            {
                TetConstraint currConstraint = (TetConstraint)this.constraints[i];
                float w_i = currConstraint.w;
                Matrix<float> S_i = currConstraint.S;

                this.leftMatrix += (w_i * S_i.Transpose() * A_transpose * A * S_i);
            }

            // TODO decomposition
            this.cholesky = this.leftMatrix.Cholesky();
        }

        private void UpdateConstraintLeftMatrix()
        {
            Matrix<float> A = this.CreateAMatrix();
            Matrix<float> A_transpose = A.Transpose();

            for (int i = 0; i < this.constraints.Count; i++)
            {
                TetConstraint currConstraint = (TetConstraint)this.constraints[i];
                if (currConstraint.leftMatrix != null)
                {
                    currConstraint.leftMatrix.Clear();
                }

                float w_i = currConstraint.w;
                Matrix<float> S_i_transpose = currConstraint.S.Transpose();

                currConstraint.SetLeftMatrix(w_i * S_i_transpose * A_transpose * A);
            }
        }

        // see https://www.youtube.com/watch?v=VoVTyMRF67A... at 57:00
        private Matrix<float> CreateAMatrix()
        {
            Matrix<float> A = Matrix<float>.Build.Dense(3 * 4, 3 * 4);

            // update A and B (assume they are equal)
            for (int row = 0; row < A.RowCount; row++)
            {
                for (int col = 0; col < A.ColumnCount; col++)
                {
                    if (row == col) // diagonal elements
                    {
                        A[row, col] = 1f - (1f / 4f);
                    }
                    else
                    {
                        A[row, col] = 0f - (1f / 4f);
                    }
                }
            }
            // Matrix<float> A = Matrix<float>.Build.DiagonalIdentity(3 * 4, 3 * 4);
            return A;
        }

        private void SetFromTetMesh(TetMesh mesh, ref Vector<float> x)
        {
            //x = Vector<float>.Build.Dense(mesh.vertices_init.Count * 3);
            for (int i = 0; i < mesh.vertices_init.Count; i++)
            {
                x[i * 3 + 0] = mesh.vertices_init[i][0];
                x[i * 3 + 1] = mesh.vertices_init[i][1];
                x[i * 3 + 2] = mesh.vertices_init[i][2];
            }
        }

        private void CreateTetConstraints()
        {
            for (int i = 0; i < this.mesh.tetrahedra_init.Count; i++)
            {
                TetMesh.Vector4i tet = this.mesh.tetrahedra_init[i];
                TetConstraint c = new TetConstraint(1, ref this.q_n, tet[0], tet[1], tet[2], tet[3]);
                c.range = this.strainRange;
                this.constraints.Add(c);
            }
        }
    }
}