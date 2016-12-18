using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class PosConstraint : Constraint
    {
        // general for any constraint
        public int index { get; private set; }
        public float w { get; private set; }
        public Matrix<float> S { get; private set; }
        public Matrix<float> leftMatrix { get; private set; }
        public Vector<float> projection;

        public PosConstraint(float weight, ref Vector<float> x, int p0)
        {
            this.index = p0;
            //
            this.w = weight;
            this.UpdateSMatrix(x.Count);
            this.projection = Vector<float>.Build.Dense(3 * 1);
        }

        public void SetLeftMatrix(Matrix<float> matrix)
        {
            this.leftMatrix = matrix;
        }

        public void UpdateSMatrix(int numDOF)
        {
            // update S
            Tuple<int, int, float>[] tuples = new Tuple<int, int, float>[1 * 3];

            tuples[0] = new Tuple<int, int, float>(0, this.index * 3 + 0, 1);
            tuples[1] = new Tuple<int, int, float>(1, this.index * 3 + 1, 1);
            tuples[2] = new Tuple<int, int, float>(2, this.index * 3 + 2, 1);

            this.S = Matrix<float>.Build.SparseOfIndexed(3 * 4, numDOF, tuples);
        }

        public override void Project(ref Vector<float> x)
        {
            Vector<float> p0 = x.SubVector(this.index * 3, 3);

            // 1. Project
            this.projection[0] = x[this.index * 3 + 0];
            this.projection[1] = x[this.index * 3 + 1];
            this.projection[2] = x[this.index * 3 + 2];
        }
    }
}