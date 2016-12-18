using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PhysicallyBasedAnimations;

public class TetSimulation : MonoBehaviour
{
    public float fps = 60;
    private float dt;

    public TetMesh mesh;
    public ProjectiveDynamics pDynamics;

    public AnchorForce anchorForce;
    public GameObject interactingParticle;
    public GameObject moveTo;

    private LineRenderer lineRen;
    private float initDist;

    public float distThreshold = 0.1f;

    public AudioSource audio;

    void Awake()
    {
        this.lineRen = this.GetComponent<LineRenderer>();

        this.dt = 1f / fps;
        Time.fixedDeltaTime = this.dt; // change how fast the FixedUpdate is called. 
        Application.targetFrameRate = (int) fps;
        Debug.Log("fps " + this.fps);
        Debug.Log("dt " + this.dt);
        
        //
        this.pDynamics.Init(this.mesh, 1f, this.dt);
    }

    private int GetClosestSurfaceVertexIndex(Vector3 pos)
    {
        int idx = -1;
        float minDist = this.distThreshold;
        for (int i = 0; i < this.mesh.surfaceVertices_init.Length; i++)
        {
            //if (this.mesh.surfaceVertices_init[i])
            {
                float dist = Vector3.Distance(pos, this.mesh.vertices_init[i]);
               // Debug.Log(i + " " + dist);
                if (dist < minDist)
                {
                    minDist = dist;
                    idx = i;
                }
            }
        }
        return idx;
    }
    

    void Update()
    {
        // pointer
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool didHit = false;
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            didHit = true;
            this.lineRen.SetPosition(0, ray.origin);
            this.lineRen.SetPosition(1, hit.point);
        }

        //
        if (Input.GetMouseButtonDown(0)) // find the particle to interact with...
        {
            if (didHit)
            {
                this.anchorForce.idx = this.GetClosestSurfaceVertexIndex(hit.point);
                if (this.anchorForce.idx > 0)
                {
                    this.interactingParticle.SetActive(true);
                    this.interactingParticle.transform.position = this.pDynamics.GetPosOfParticleAt(this.anchorForce.idx);

                    this.moveTo.transform.position = this.interactingParticle.transform.position;
                    this.moveTo.SetActive(true);
                    this.anchorForce.SetAnchorToPoint(this.moveTo.transform.position);
                    this.initDist = Vector3.Distance(ray.origin, this.interactingParticle.transform.position);
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (this.anchorForce.idx > 0)
            {
                this.interactingParticle.transform.position = this.pDynamics.GetPosOfParticleAt(this.anchorForce.idx);

                Vector3 pos = ray.origin + ray.direction * this.initDist;
                this.moveTo.transform.position = pos;
                this.anchorForce.SetAnchorToPoint(pos);

                this.lineRen.SetPosition(0, ray.origin);
                this.lineRen.SetPosition(1, pos);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            this.anchorForce.idx = -1;
            this.interactingParticle.SetActive(false);
            this.moveTo.SetActive(false);
        }

        { // Step   
            this.pDynamics.step();
            //
            int cnt = 0;
            for (int i = 0; i < this.pDynamics.constraints.Count; i++)
            {
               if (( (TetConstraint)this.pDynamics.constraints[i]).wasViolated)
                {
                    cnt += 1;
                }
            }
            this.audio.volume = (float) cnt / this.pDynamics.constraints.Count;

            // Update the Mesh
            this.mesh.UpdateMesh(ref this.pDynamics.q_n1);
        }
    }
}
