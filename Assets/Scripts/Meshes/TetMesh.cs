﻿using UnityEngine;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using MathNet.Numerics.LinearAlgebra;

namespace PhysicallyBasedAnimations
{
    public class TetMesh : MonoBehaviour
    {
        public string meshDirPath = "Meshes";
        public string filename = "bunny";

        // read from the files
        public List<Vector3> vertices_init = new List<Vector3>();
        public List<Vector3i> triangles_init = new List<Vector3i>();
        public List<Vector3i> surfaceTriangles_init = new List<Vector3i>();
        public List<Vector4i> tetrahedra_init = new List<Vector4i>();

        ///
        private Mesh mesh;
        private Vector3[] vertices_new; // update based on a simulation

        public void Init()
        {
            // clear...
            this.vertices_init.Clear();
            this.triangles_init.Clear();
            this.surfaceTriangles_init.Clear();
            this.tetrahedra_init.Clear();

            // open the node and element files.
            this.OpenFile(Application.dataPath + "/" + this.meshDirPath + "/" + this.filename);
            Debug.Log("- # of vertices: " + this.vertices_init.Count);
            Debug.Log("- # of tetrahedra: " + this.tetrahedra_init.Count);
            Debug.Log("- # of tringles: " + this.triangles_init.Count);
            Debug.Log("- # of surface triangles: " + this.surfaceTriangles_init.Count);

            // make an mesh (just from the surface triangles).
            Mesh mesh = new Mesh();
            this.GetComponent<MeshFilter>().mesh = mesh;
            this.mesh = mesh;
            this.ResetMesh();

            // update the collider.
            this.GetComponent<MeshCollider>().sharedMesh = mesh;

            //
            this.vertices_new = new Vector3[this.vertices_init.Count];
        }


        /// <summary>
        /// Reset the mesh from the original data from the node and element files.
        /// </summary>
        public void ResetMesh()
        {
            Vector3[] newVertices = vertices_init.ToArray();
            int[] newTriangles = new int[this.surfaceTriangles_init.Count * 3];

            for (int t = 0; t < this.surfaceTriangles_init.Count; t++)
            {
                Vector3i tri = this.surfaceTriangles_init[t];
                newTriangles[t * 3] = tri[0];
                newTriangles[t * 3 + 1] = tri[1];
                newTriangles[t * 3 + 2] = tri[2];
            }

            this.mesh.vertices = newVertices;
            this.mesh.triangles = newTriangles;

            this.mesh.RecalculateBounds();
            this.mesh.RecalculateNormals();
        }

        /// <summary>
        /// Update the vertices of the mesh from the particle system.
        /// ASSUME that there is no new particle added or removed from the original set.
        /// ASSUME that there is no change in surface triangles.
        /// </summary>
        /// <param name="ps"></param>
        public void UpdateMesh(ParticleSystem ps)
        {
            List<ParticleSystem.Particle> particles = ps.particles;

            // update vertices
            for (int i = 0; i < particles.Count; i++)
            {
                ParticleSystem.Particle p = particles[i];
                this.vertices_new[i][0] = p.x[0];
                this.vertices_new[i][1] = p.x[1];
                this.vertices_new[i][2] = p.x[2];
            }
            // no need to re-update surface triangles

            this.mesh.vertices = this.vertices_new;

            this.mesh.RecalculateBounds();
            this.mesh.RecalculateNormals();
        }

        /*void OnRenderObject()
       {
           GL.PushMatrix();
           GL.MultMatrix(transform.localToWorldMatrix);
           GL.Begin(GL.TRIANGLES);
           for (int t = 0; t < triangles.Count; t++)
           {
               Vector3i tri = triangles[t];
               for (int v = 0; v < 3; v++)
               {
                   Vector3 vertex = this.vertices[tri[v]];
                   GL.Vertex3(vertex[0], vertex[1], vertex[2]);
               }
           }
           GL.End();
           GL.PopMatrix();
       }*/

        private void OpenFile(string filePath)
        {
            // clear...
            this.vertices_init.Clear();
            this.triangles_init.Clear();
            this.surfaceTriangles_init.Clear();
            this.tetrahedra_init.Clear();

            Debug.Log("Reading a tetrahedra file... " + this.filename);
            { // nodes
              // check if the node file exists.
                string nodefile = filePath + ".node";
                if (!File.Exists(nodefile))
                {
                    Debug.LogError("Error: failed to open file " + nodefile);
                    return;
                }
                // read the file.
                using (FileStream file = new FileStream(nodefile, FileMode.Open))
                using (StreamReader reader = new StreamReader(file))
                {
                    string line = reader.ReadLine();
                    string[] numbers = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    int numOfNodes = int.Parse(numbers[0]);
                    for (int i = 0; i < numOfNodes; i++)
                    {
                        line = reader.ReadLine();
                        numbers = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        Vector3 vert = new Vector3(float.Parse(numbers[1]), float.Parse(numbers[2]), float.Parse(numbers[3]));
                        this.vertices_init.Add(vert);
                    }
                }
            } // end-nodes
            Debug.Log("- done reading the node file.");

            { // tets
                string elefile = filePath + ".ele";
                if (!File.Exists(elefile))
                {
                    Debug.LogError("Error: failed to open file " + elefile);
                    return;
                }
                // read the file.
                using (FileStream file = new FileStream(elefile, FileMode.Open))
                using (StreamReader reader = new StreamReader(file))
                {
                    string line = reader.ReadLine();
                    string[] numbers = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    int numOfEles = int.Parse(numbers[0]);
                    for (int i = 0; i < numOfEles; i++)
                    {
                        line = reader.ReadLine();
                        numbers = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        Vector4i tet = new Vector4i(int.Parse(numbers[1]), int.Parse(numbers[2]), int.Parse(numbers[3]), int.Parse(numbers[4]));
                        this.tetrahedra_init.Add(tet);
                    }
                }
            } // end-tets
            Debug.Log("- done reading the ele file.");

            { // triangles            
                List<bool> isSurfaceTriangle = new List<bool>();
                for (int t = 0; t < this.tetrahedra_init.Count; t++)
                {
                    Vector4i tet = this.tetrahedra_init[t];
                    Vector3i[] tris = {
                    MakeTriangle (tet [0], tet [2], tet [1]),
                    MakeTriangle (tet [0], tet [1], tet [3]),
                    MakeTriangle (tet [0], tet [3], tet [2]),
                    MakeTriangle (tet [1], tet [2], tet [3])
                };

                    for (int r = 0; r < 4; r++)
                    {
                        Vector3i tri = tris[r];
                        Vector3i revTri = new Vector3i(tri[0], tri[2], tri[1]);
                        //int revIndex = this.triangles.IndexOf (revTri); // too slow
                        int revIndex = this.GetIndexOf(this.triangles_init, revTri); // much faster to use our own implementation.
                        if (revIndex == -1)
                        { // not found
                            this.triangles_init.Add(tri);
                            isSurfaceTriangle.Add(true);
                        }
                        else {
                            isSurfaceTriangle[revIndex] = false;
                        }
                    }
                }

                for (int r = 0; r < this.triangles_init.Count; r++)
                {
                    if (isSurfaceTriangle[r])
                    {
                        this.surfaceTriangles_init.Add(this.triangles_init[r]);
                    }
                }

            } // end-triangles
            Debug.Log("- done making triangles.");
        }

        // as described in https://www.dotnetperls.com/array-indexof
        // it can be much fast to implement out own index of function.
        private int GetIndexOf(List<Vector3i> list, Vector3i element)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if ((list[i][0] == element[0]) && (list[i][1] == element[1]) && (list[i][2] == element[2]))
                {
                    return i;
                }
            }
            return -1;
        }


        private Vector3i MakeTriangle(int i, int j, int k)
        {
            if (i < j && i < k)
            {
                return new Vector3i(i, j, k);
            }
            else if (j < i && j < k)
            {
                return new Vector3i(j, k, i);
            }
            else {
                return new Vector3i(k, i, j);
            }
        }


        public struct Vector3i
        {
            public int[] values;

            public Vector3i(int x, int y, int z)
            {
                this.values = new int[3];
                this.values[0] = x;
                this.values[1] = y;
                this.values[2] = z;
            }

            public int this[int i]
            {
                get
                {
                    return values[i];
                }
                set
                {
                    values[i] = value;
                }
            }
        }


        public struct Vector4i
        {
            public int[] values;

            public Vector4i(int x, int y, int z, int w)
            {
                this.values = new int[4];
                this.values[0] = x;
                this.values[1] = y;
                this.values[2] = z;
                this.values[3] = w;
            }

            public int this[int i]
            {
                get
                {
                    return values[i];
                }
                set
                {
                    values[i] = value;
                }
            }
        }

    }
}