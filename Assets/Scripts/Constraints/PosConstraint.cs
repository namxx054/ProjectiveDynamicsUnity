using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class PosConstraint : Constraint
    {
        public PosConstraint(float weight, ref Vector<float> x, List<int> p)
        {
            base.indices = p.ToArray();
            //
            base.w = weight;
            base.projection = Vector<float>.Build.Dense(3 * 1);
        }


        public override void SetLeftMatrix(Matrix<float> matrix)
        {
            this.leftMatrix = matrix;
        }

        public override void Project(ref Vector<float> x)
        {
            for (int i = 0; i < base.indices.Length; i++)
            {
                int index = base.indices[i];
                this.projection[0] = x[index * 3 + 0];
                this.projection[1] = x[index * 3 + 1];
                this.projection[2] = x[index * 3 + 2];
            }
        }
    }
}