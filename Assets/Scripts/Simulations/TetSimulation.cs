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

    public AnchorForce mouseForce;
    public GameObject mouseParticle;

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
        if (Input.GetMouseButtonDown(0)) // find the particle to interact with...
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                this.mouseForce.p = this.ps.GetClosestParticle(hit.point, 0.5f);
               // this.mouseForce.x = new Vector3(this.mouseForce.p.x[0], this.mouseForce.p.x[1], this.mouseForce.p.x[2]);
                if (this.mouseForce.p != null)
                {
                    this.mouseParticle.SetActive(true);
                    this.mouseParticle.transform.position = new Vector3(this.mouseForce.p.x[0], this.mouseForce.p.x[1], this.mouseForce.p.x[2]);
                    this.mouseForce.p = null;
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
        }
        else if (Input.GetMouseButtonUp(0))
        {
            this.mouseParticle.SetActive(false);
        }

        { // Step
            for (int i = 0; i < this.subSteps; i++)
            {
                integrator.step(dt);
            }
            // Update the Mesh
            this.mesh.UpdateMesh(this.ps);
        }
    }
}
