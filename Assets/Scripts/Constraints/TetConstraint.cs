using UnityEngine;
using System.Collections;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class TetConstraint : Constraint
    {
        public Vector2 range; // for strain limiting
        
        public Matrix<float> D_m { get; private set; }
        public Matrix<float> D_m_inv { get; private set; }
        public float volume { get; private set; }

        public bool wasViolated { get; private set; }


        public TetConstraint(float weight, ref Vector<float> x, int p0, int p1, int p2, int p3)
        {
            base.indices = new int[4];
            base.indices[0] = p0;
            base.indices[1] = p1;
            base.indices[2] = p2;
            base.indices[3] = p3;
            //
            base.w = weight;
            base.projection = Vector<float>.Build.Dense(3 * 4);
            // compute D_m and D_m_inv
            this.D_m = Matrix<float>.Build.Dense(3, 3);
            this.D_m.SetColumn(0, x.SubVector(p1 * 3, 3) - x.SubVector(p0 * 3, 3)); // p1 - p0
            this.D_m.SetColumn(1, x.SubVector(p2 * 3, 3) - x.SubVector(p0 * 3, 3)); // p2 - p0
            this.D_m.SetColumn(2, x.SubVector(p3 * 3, 3) - x.SubVector(p0 * 3, 3)); // p3 - p0
            this.D_m_inv = this.D_m.Inverse();
            // compute volume
            this.volume = Mathf.Abs(D_m.Determinant()) / 6f;
        }


        public override void SetLeftMatrix(Matrix<float> matrix)
        {
            this.leftMatrix = matrix;
        }

        public override void Project(ref Vector<float> x)
        {
            Vector<float> p0 = x.SubVector(base.indices[0] * 3, 3);
            Vector<float> p1 = x.SubVector(base.indices[1] * 3, 3);
            Vector<float> p2 = x.SubVector(base.indices[2] * 3, 3);
            Vector<float> p3 = x.SubVector(base.indices[3] * 3, 3);

            // 1. Compute D_s (deformation)
            Matrix<float> D_s = Matrix<float>.Build.Dense(3, 3);
            D_s.SetColumn(0, p1 - p0);
            D_s.SetColumn(1, p2 - p0);
            D_s.SetColumn(2, p3 - p0);
            // 2. Compute F (deformation gradient)
            Matrix<float> F = D_s * this.D_m_inv;
            // 3. SVD on F (to get eigen values)
            var svd = F.Svd(true);
            // 4. Clamp - Strain Limiting - also indicate the constraint was violated
            this.wasViolated = false;
            if (svd.S[0] < this.range.x || svd.S[0] > this.range.y)
            {
                this.wasViolated = true;
                svd.W[0, 0] = Mathf.Clamp(svd.S[0], this.range.x, this.range.y);
            }
            if (svd.S[1] < this.range.x || svd.S[1] > this.range.y)
            {
                this.wasViolated = true;
                svd.W[1, 1] = Mathf.Clamp(svd.S[1], this.range.x, this.range.y);
            }
            if (svd.S[2] < this.range.x || svd.S[2] > this.range.y)
            {
                this.wasViolated = true;
                svd.W[2, 2] = Mathf.Clamp(svd.S[2], this.range.x, this.range.y);
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
        }
    }
}