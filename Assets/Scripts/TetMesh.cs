using UnityEngine;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using MathNet.Numerics.LinearAlgebra;

public class TetMesh : MonoBehaviour
{
	public string meshDirPath = "Meshes";
	public string filename = "bunny";

	//
	private List<Vector3> vertices = new List<Vector3> ();
	//
	public List<Vector3i> triangles = new List<Vector3i> ();
	public List<Vector3i> surfaceTriangles = new List<Vector3i> ();
	//
	public List<Vector4i> tetrahedra = new List<Vector4i> ();

    private Mesh mesh;
    private MeshCollider collider;

    void Start ()
	{
		this.OpenFile (Application.dataPath + "/" + this.meshDirPath + "/" + this.filename);
		Debug.Log ("- # of vertices: " + this.vertices.Count);
		Debug.Log ("- # of tetrahedra: " + this.tetrahedra.Count);
        Debug.Log("- # of tringles: " + this.triangles.Count);
        Debug.Log("- # of surface triangles: " + this.surfaceTriangles.Count);

        // make an mesh (just from the surface triangles)
        Mesh mesh = new Mesh();
        this.GetComponent<MeshFilter>().mesh = mesh;
        this.mesh = mesh;
        this.UpdateMesh();

        this.collider = this.GetComponent<MeshCollider>();
        this.collider.sharedMesh = this.mesh;
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

    void Update()
    {
        this.UpdateMesh();
    }

    public void UpdateMesh()
    {
        Vector3[] newVertices = vertices.ToArray();
        int[] newTriangles = new int[this.surfaceTriangles.Count * 3];

        for (int t = 0; t < this.surfaceTriangles.Count; t++)
        {
            Vector3i tri = this.surfaceTriangles[t];
            newTriangles[t * 3] = tri[0];
            newTriangles[t * 3 + 1] = tri[1];
            newTriangles[t * 3 + 2] = tri[2];
        }

        this.mesh.vertices = newVertices;
        this.mesh.triangles = newTriangles;

        this.mesh.RecalculateBounds();
        this.mesh.RecalculateNormals();
    }
    
    private void OpenFile (string filePath)
	{
		// clear...
		this.vertices.Clear ();
		this.triangles.Clear ();
		this.surfaceTriangles.Clear ();
		this.tetrahedra.Clear ();

        Debug.Log("Reading a tetrahedra file... " + this.filename);
        { // nodes
			// check if the node file exists.
			string nodefile = filePath + ".node";
			if (!File.Exists (nodefile)) {
				Debug.LogError ("Error: failed to open file " + nodefile);
				return;
			}
            // read the file.
            using (FileStream file = new FileStream (nodefile, FileMode.Open))
			using (StreamReader reader = new StreamReader (file)) {
				string line = reader.ReadLine ();
				string[] numbers = line.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				int numOfNodes = int.Parse (numbers [0]);
				for (int i = 0; i < numOfNodes; i++) {
					line = reader.ReadLine ();
					numbers = line.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					Vector3 vert = new Vector3 (float.Parse (numbers [1]), float.Parse (numbers [2]), float.Parse (numbers [3]));
					this.vertices.Add (vert);
				}
            }
		} // end-nodes
        Debug.Log("- done reading the node file.");

		{ // tets
			string elefile = filePath + ".ele";
			if (!File.Exists (elefile)) {
				Debug.LogError ("Error: failed to open file " + elefile);
				return;
			}
            // read the file.
            using (FileStream file = new FileStream (elefile, FileMode.Open))
			using (StreamReader reader = new StreamReader (file)) {
				string line = reader.ReadLine ();
				string[] numbers = line.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				int numOfEles = int.Parse (numbers [0]);
				for (int i = 0; i < numOfEles; i++) {
					line = reader.ReadLine ();
					numbers = line.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Vector4i tet = new Vector4i(int.Parse(numbers[1]), int.Parse(numbers[2]), int.Parse(numbers[3]), int.Parse(numbers[4]));
					this.tetrahedra.Add (tet);
				}
			}
        } // end-tets
        Debug.Log("- done reading the ele file.");

        { // triangles            
            List<bool> isSurfaceTriangle = new List<bool> ();
			for (int t = 0; t < this.tetrahedra.Count; t++) {
                Vector4i tet = tetrahedra [t];
                Vector3i[] tris = {
					MakeTriangle (tet [0], tet [2], tet [1]),
                    MakeTriangle (tet [0], tet [1], tet [3]),
                    MakeTriangle (tet [0], tet [3], tet [2]),
                    MakeTriangle (tet [1], tet [2], tet [3])
				};

				for (int r = 0; r < 4; r++) {
                    Vector3i tri = tris [r];
                    Vector3i revTri = new Vector3i(tri[0], tri[2], tri[1]);
					//int revIndex = this.triangles.IndexOf (revTri); // too slow
                    int revIndex = this.GetIndexOf(this.triangles, revTri); // much faster to use our own implementation.
					if (revIndex == -1) { // not found
						this.triangles.Add (tri);
						isSurfaceTriangle.Add (true);
					} else {
						isSurfaceTriangle [revIndex] = false;
					}
				}
			}

            for (int r = 0; r < this.triangles.Count; r++) {
				if (isSurfaceTriangle [r]) {
					this.surfaceTriangles.Add (triangles [r]);
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


	private Vector3i MakeTriangle (int i, int j, int k)
	{
		if (i < j && i < k) {
            return new Vector3i(i, j, k);
        } else if (j < i && j < k) {
            return new Vector3i(j, k, i);
        } else {
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