using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace PhysicallyBasedAnimations
{
    public class PBD : MonoBehaviour
    {
        public enum AandBType { CANCEL_GLOBAL = 0, IDENTITY = 1 }
        
        public AandBType aAndBType;

        public int iterationNumber = 1;

        public Vector2 strainRange;

        public TetMesh mesh; // TODO make it private
        
        public GravityForce f_gravity;
        public AnchorForce f_anchor;

        // low-level variables
        public Vector<float> q_n, v_n, f_n; // current states
        public Vector<float> q_n1, v_n1; // next states
        private Matrix<float> M; // assume fixed !!! M.inverse() takes forever, if M is a SparseDiagonal !!!
        private float h;
        private float t;
        // higher-level variables
        private List<Constraint> constraints;
        private Cholesky<float> leftMatrixCholesky;
        private Vector<float> rightVector;

        void Start()
        {
            this.mesh.Init();
            
            this.Init(this.mesh, 0.5f, 0.2f);
        }

        public void Init(TetMesh mesh, float pMass, float h)
        {
            Debug.Log("Initializing.... Projective Dynamics");
            this.mesh = mesh;
            Debug.Log("- Mesh: " + this.mesh.filename);

            // set low-level variables
            int n = mesh.GetDOF();
            Debug.Log("- # of DOF: " + n);
            this.q_n = Vector<float>.Build.Sparse(n);
            this.SetQFromTetMesh(mesh, ref this.q_n);
            this.v_n = Vector<float>.Build.Sparse(n);
            this.f_n = Vector<float>.Build.Sparse(n);
            this.M = Matrix<float>.Build.Diagonal(n, n, pMass);
            this.h = h;
            // set higher-level variables
            //// set constraints
            this.constraints = new List<Constraint>();
            this.constraints.AddRange(this.CreateTetConstraintFromMesh(this.mesh, ref this.q_n, this.strainRange));
            this.UpdateConstraintLeftMatrix();
            Debug.Log("- # of constraints: " + this.constraints.Count);
            //// set leftMatrixCholesky
            this.UpdateLeftMatrix();

            //Debug.Log(this.CreateAMatrix(this.constraints[0], this.aAndBType).ToString("f2"));
            //Debug.Log(this.CreateSMatrix(this.constraints[0], this.mesh.GetDOF()).ToString("f2"));
            
        }


        private void UpdateLeftMatrix()
        {
            Matrix<float> leftMatrix = (this.M / (h * h));

            for (int i = 0; i < this.constraints.Count; i++)
            {
                Constraint constraint = this.constraints[i];

                float w_i = constraint.w;
                Matrix<float> A_i = this.CreateAMatrix(constraint, this.aAndBType);
                Matrix<float> S_i = this.CreateSMatrix(constraint, this.mesh.GetDOF());

                leftMatrix += (w_i * S_i.Transpose() * A_i.Transpose() * A_i * S_i);
            }
            Debug.Log("left matrix:\n" + leftMatrix.ToString("f2"));
            this.leftMatrixCholesky = leftMatrix.Cholesky();
        }

        private void UpdateConstraintLeftMatrix()
        {
            for (int i = 0; i < this.constraints.Count; i++)
            {
                Constraint constraint = this.constraints[i];

                float w_i = constraint.w;
                Matrix<float> A_i = this.CreateAMatrix(constraint, this.aAndBType);
                Matrix<float> S_i_transpose = this.CreateSMatrix(constraint, this.mesh.GetDOF()).Transpose();

                constraint.SetLeftMatrix(w_i * S_i_transpose * A_i.Transpose() * A_i);
            }
        }

        private void SetQFromTetMesh(TetMesh mesh, ref Vector<float> x)
        {
            for (int i = 0; i < mesh.vertices_init.Count; i++)
            {
                x[i * 3 + 0] = mesh.vertices_init[i][0];
                x[i * 3 + 1] = mesh.vertices_init[i][1];
                x[i * 3 + 2] = mesh.vertices_init[i][2];
            }
        }

        private List<Constraint> CreateTetConstraintFromMesh(TetMesh mesh, ref Vector<float> q, Vector2 strainRange)
        {
            List<Constraint> constraints = new List<Constraint>();
            for (int i = 0; i < mesh.tetrahedra_init.Count; i++)
            {
                TetMesh.Vector4i tet = mesh.tetrahedra_init[i];
                TetConstraint c = new TetConstraint(1, ref q, tet[0], tet[1], tet[2], tet[3]);
                c.range = strainRange;
                constraints.Add(c);
            }
            return constraints;
        }
        
        // get A matrix; specific to a tetrahedron constraint.
        // assume A and B are equal.
        // see https://www.youtube.com/watch?v=VoVTyMRF67A... at 57:00
        private Matrix<float> CreateAMatrix(Constraint constraint, AandBType type)
        {
            int[] indices = constraint.indices;
            float diagVal = 1f - (1f / indices.Length);
            float nonDiagVal = 0f - (1f / indices.Length);

            if (aAndBType == AandBType.IDENTITY)
            {
                diagVal = 1f;
                // nonDiagVal is not used;
            }

            Matrix<float> A = Matrix<float>.Build.Sparse(3 * 4, 3 * 4);
            for (int row = 0; row < A.RowCount; row++)
            {
                for (int col = 0; col < A.ColumnCount; col++)
                {
                    if (row == col) // diagonal elements
                    {
                        A[row, col] = diagVal;
                    }
                    else if (type == AandBType.CANCEL_GLOBAL) // non-diagonal elements
                    {
                        A[row, col] = nonDiagVal;
                    }
                }
            }
            return A;
        }

        private Matrix<float> CreateSMatrix(Constraint constraint, int dof)
        {
            int[] indices = constraint.indices;

            Matrix<float> S = Matrix<float>.Build.Sparse(3 * indices.Length, dof);

            for (int i = 0; i < indices.Length; i++)
            {
                S[i * 3 + 0, indices[i] * 3 + 0] = 1;
                S[i * 3 + 1, indices[i] * 3 + 1] = 1;
                S[i * 3 + 2, indices[i] * 3 + 2] = 1;
            }

            return S;
        }

        /*public void Init(TetMesh mesh, float pMass, float h)
        {
            this.mesh = mesh;
            mesh.Init();


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

        


       

        



        

        */
    }
}