using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PhysicallyBasedAnimations;

public class TetSimulation : MonoBehaviour
{
    public float fps = 60;
    public int subSteps = 10;
    private float dt;

    public TetMesh mesh;
    public PhysicallyBasedAnimations.ParticleSystem ps;
    public SymplecticEuler integrator;

    void Start()
    {
        this.dt = 1f / fps / subSteps;
        Time.fixedDeltaTime = this.dt; // change how fast the FixedUpdate is called. 
        Application.targetFrameRate = (int) fps;
        Debug.Log("fps " + this.fps);
        Debug.Log("sub-steps " + this.subSteps);
        Debug.Log("dt " + this.dt);

        this.mesh.Init(); // load the mesh from the files.
        this.ps.Init(this.mesh, 1f); // update the particle system with the mesh.
        this.integrator.Init(ps); // set integrator to the particle system.
    }

    void FixedUpdate()
    {
        {
            // Step
            for (int i = 0; i < this.subSteps; i++)
            {
                integrator.step(dt);
            }
            // Update the Mesh
            this.mesh.UpdateMesh(this.ps);
        }
    }
}
