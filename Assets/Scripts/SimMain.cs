using UnityEngine;
using System.Collections;

using PhysicallyBasedAnimations;

public class SimMain : MonoBehaviour {

    public float fps = 60;
    public int subSteps = 10;
    private float dt;
    
    public SymplecticEuler integrator;
    public TetMesh mesh;
    
    void Start () {
	    this.dt = 1f / fps / subSteps;
    }
	
	void Update () {
        StartCoroutine(Step(this.dt));
    }
    

    private IEnumerator Step(float dt)
    {
        // Step
        for (int i = 0; i < this.subSteps; i++)
        {
            integrator.step(dt);
        }
        // Update the Mesh

        yield return new WaitForSeconds(dt);
    }
}
