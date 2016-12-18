using UnityEngine;
using System.Collections;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class ConstraintTestMain : MonoBehaviour
    {
        public TetMesh mesh;
        public TetMesh projectedMesh;
        public TetConstraintTest constraint;
        public Vector2 strainRange;

        public TetConstraintUI ui;
        public int interactingPtIndx = 0;
        public float moveAmt = 0.01f;

        private Vector<float> q_n;
        private Vector<float> p_n; // projected

        public bool checkInverse = false;

        void Start()
        {
            this.mesh.Init(); // load the mesh from the files.
            this.projectedMesh.Init();

            { // create q_n.
                this.SetFromTetMesh(this.mesh, ref this.q_n);
                this.SetFromTetMesh(this.projectedMesh, ref this.p_n);
            }

            { // create a constraint
                TetMesh.Vector4i vec4i = this.mesh.tetrahedra_init[0];
                this.constraint = new TetConstraintTest(1f, ref q_n, vec4i[0], vec4i[1], vec4i[2], vec4i[3]);
                this.constraint.range = this.strainRange;
                this.constraint.ui = this.ui;
                this.constraint.useInverse = this.checkInverse;
            }

        }

        void Update()
        {
            this.ProcessKeyInput(this.interactingPtIndx, this.moveAmt); // update q_n based on user inputs.

            this.constraint.Project(ref this.q_n);
            for (int i = 0; i < this.constraint.indices.Length; i++) // apply the projection
            {
                this.p_n[this.constraint.indices[i] * 3 + 0] = this.constraint.projection[i * 3 + 0];
                this.p_n[this.constraint.indices[i] * 3 + 1] = this.constraint.projection[i * 3 + 1];
                this.p_n[this.constraint.indices[i] * 3 + 2] = this.constraint.projection[i * 3 + 2];
            }

            this.mesh.UpdateMesh(ref this.q_n);
            this.projectedMesh.UpdateMesh(ref this.p_n);
        }

        private void SetFromTetMesh(TetMesh mesh, ref Vector<float> x)
        {
            x = Vector<float>.Build.Dense(mesh.vertices_init.Count * 3);
            for (int i = 0; i < mesh.vertices_init.Count; i++)
            {
                x[i * 3 + 0] = mesh.vertices_init[i][0];
                x[i * 3 + 1] = mesh.vertices_init[i][1];
                x[i * 3 + 2] = mesh.vertices_init[i][2];
            }
        }
    

        private void ProcessKeyInput(int ptIdx, float amt)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.UpArrow)) // move forward => z
            {
                this.q_n[ptIdx * 3 + 2] += amt;
            }
            if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.DownArrow)) // move backward => z
            {
                this.q_n[ptIdx * 3 + 2] -= amt;

            }
            if (Input.GetKey(KeyCode.RightArrow)) // move right => x
            {
                this.q_n[ptIdx * 3 + 0] += amt;
            }
            if (Input.GetKey(KeyCode.LeftArrow)) // move left => x
            {
                this.q_n[ptIdx * 3 + 0] -= amt;

            }
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.UpArrow)) // move up => y
            {
                this.q_n[ptIdx * 3 + 1] += amt;
            }
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.DownArrow)) // move down => y
            {
                this.q_n[ptIdx * 3 + 1] -= amt;
            }
        }
    }
}