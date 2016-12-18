using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class TetConstraintTest
    {
        public TetConstraintUI ui;

        public Vector2 range; // for strain limiting

        // general for any constraint
        public int[] indices { get; private set; }
        public float w { get; private set; }
        public Matrix<float> S { get; private set; }
        public Matrix<float> leftMatrix { get; private set; }
        public Vector<float> projection { get; private set; }

        // specific to this constraint
        public Matrix<float> D_m { get; private set; }
        public Matrix<float> D_m_inv { get; private set; }
        public float volume { get; private set; }

        public bool useInverse = false;

        public TetConstraintTest(float weight, ref Vector<float> x, int p0, int p1, int p2, int p3)
        {
            this.indices = new int[4];
            this.indices[0] = p0;
            this.indices[1] = p1;
            this.indices[2] = p2;
            this.indices[3] = p3;
            //
            this.w = weight;
            this.UpdateSMatrix(x.Count);
            this.projection = Vector<float>.Build.Dense(3 * 4);
            // compute D_m and D_m_inv.
            this.D_m = Matrix<float>.Build.Dense(3, 3);
            this.D_m.SetColumn(0, x.SubVector(p1 * 3, 3) - x.SubVector(p0 * 3, 3)); // p1 - p0
            this.D_m.SetColumn(1, x.SubVector(p2 * 3, 3) - x.SubVector(p0 * 3, 3)); // p2 - p0
            this.D_m.SetColumn(2, x.SubVector(p3 * 3, 3) - x.SubVector(p0 * 3, 3)); // p3 - p0
            this.D_m_inv = this.D_m.Inverse();
            // compute volume
            this.volume = Mathf.Abs(D_m.Determinant()) / 6f;
        }

        public void SetLeftMatrix(Matrix<float> matrix)
        {
            this.leftMatrix = matrix;
        }

        public void UpdateSMatrix(int numDOF)
        {
            // update S
            Tuple<int, int, float>[] tuples = new Tuple<int, int, float>[this.indices.Length * 3];
            for (int i = 0; i < this.indices.Length; i++)
            {
                tuples[i * 3 + 0] = new Tuple<int, int, float>(i * 3 + 0, this.indices[i] * 3 + 0, 1);
                tuples[i * 3 + 1] = new Tuple<int, int, float>(i * 3 + 1, this.indices[i] * 3 + 1, 1);
                tuples[i * 3 + 2] = new Tuple<int, int, float>(i * 3 + 2, this.indices[i] * 3 + 2, 1);
            }
            this.S = Matrix<float>.Build.SparseOfIndexed(3 * 4, numDOF, tuples);
        }

        public void Project(ref Vector<float> x)
        {
            Vector<float> p0 = x.SubVector(this.indices[0] * 3, 3);
            Vector<float> p1 = x.SubVector(this.indices[1] * 3, 3);
            Vector<float> p2 = x.SubVector(this.indices[2] * 3, 3);
            Vector<float> p3 = x.SubVector(this.indices[3] * 3, 3);

            // 1. Compute D_s
            Matrix<float> D_s = Matrix<float>.Build.Dense(3, 3);
            D_s.SetColumn(0, p1 - p0);
            D_s.SetColumn(1, p2 - p0);
            D_s.SetColumn(2, p3 - p0);
            if (this.ui != null)
            {
                this.ui.volValsTxt.text = "Volume:\n" + ((D_s.Determinant()) * (1f/6f)).ToString("f2");
            }
            // 2. Compute F (deformation gradient)
            Matrix<float> F = D_s * this.D_m_inv;
            // 3. SVD on F (for getting eigen values)
            var svd = F.Svd(true);
            if (this.ui != null)
            {
                this.ui.eigenValsTxt.text = "Eigen Values:\n(" + svd.S[0].ToString("f2") + ", " + svd.S[1].ToString("f2") + ", " + svd.S[2].ToString("f2") + ")";
            }
            
            // 4. Clamp - Strain Limiting
            svd.W[0, 0] = Mathf.Clamp(svd.S[0], this.range.x, this.range.y);
            svd.W[1, 1] = Mathf.Clamp(svd.S[1], this.range.x, this.range.y);
            svd.W[2, 2] = Mathf.Clamp(svd.S[2], this.range.x, this.range.y);
            if (this.ui != null)
            {
                this.ui.eigenValsTxt.text += " => " + svd.W[0, 0].ToString("f2") + ", " + svd.W[1, 1].ToString("f2") + ", " + svd.W[2, 2].ToString("f2");
            }

            bool did = false;
            if (this.useInverse && svd.U.Determinant() * svd.VT.Transpose().Determinant() < 0.0f)
            {
                svd.W[2, 2] = -svd.W[2, 2];
                did = true;
            }
            if (this.useInverse)
            {
                this.ui.eigenValsTxt.text += did ? "\nYes" : "\nNo";
            }
            // 5. TODO handle inverse
            // 6. Update the F (deformation gradient)
            F = svd.U * svd.W * svd.VT;
            // 7. Get the new deformation

            D_s = F * this.D_m;
            // 8. Project
            Vector<float> centroid = (p0 + p1 + p2 + p3) / 4f;
            this.projection.SetSubVector(0, 3, centroid - ((D_s.Column(0) + D_s.Column(1) + D_s.Column(2)) / 4f));
            this.projection.SetSubVector(3, 3, D_s.Column(0) + this.projection.SubVector(0, 3));
            this.projection.SetSubVector(6, 3, D_s.Column(1) + this.projection.SubVector(0, 3));
            this.projection.SetSubVector(9, 3, D_s.Column(2) + this.projection.SubVector(0, 3));

            if (this.ui != null)
            {
                // before
                this.ui.p0Txt.text = "p0: (" + p0[0].ToString("f2") + ", " + p0[1].ToString("f2") + ", " + p0[2].ToString("f2") + ")";
                this.ui.p1Txt.text = "p1: (" + p1[0].ToString("f2") + ", " + p1[1].ToString("f2") + ", " + p1[2].ToString("f2") + ")";
                this.ui.p2Txt.text = "p2: (" + p2[0].ToString("f2") + ", " + p2[1].ToString("f2") + ", " + p2[2].ToString("f2") + ")";
                this.ui.p3Txt.text = "p3: (" + p3[0].ToString("f2") + ", " + p3[1].ToString("f2") + ", " + p3[2].ToString("f2") + ")";
                // after
                this.ui.p0Txt.text += " => (" + projection[0 + 0].ToString("f2") + ", " + projection[0 + 1].ToString("f2") + ", " + projection[0 + 2].ToString("f2") + ")";
                this.ui.p1Txt.text += " => (" + projection[3 + 0].ToString("f2") + ", " + projection[3 + 1].ToString("f2") + ", " + projection[3 + 2].ToString("f2") + ")";
                this.ui.p2Txt.text += " => (" + projection[6 + 0].ToString("f2") + ", " + projection[6 + 1].ToString("f2") + ", " + projection[6 + 2].ToString("f2") + ")";
                this.ui.p3Txt.text += " => (" + projection[9 + 0].ToString("f2") + ", " + projection[9 + 1].ToString("f2") + ", " + projection[9 + 2].ToString("f2") + ")";
            }

            if (this.ui != null)
            {
                this.ui.rangeTxt.text = "Range: (" + this.range[0].ToString("f2") + ", " + this.range[1].ToString("f2") + ")";
                this.ui.volValsTxt.text += " => " + ((D_s.Determinant()) * (1f / 6f)).ToString("f2");
            }
        }
    }
}