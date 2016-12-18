using UnityEngine;
using System.Collections;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class TetSimV2 : MonoBehaviour
    {
        public TetMesh mesh;

        void Start()
        {
            this.mesh.Init(); // load the mesh from the files.

        }



    }
}