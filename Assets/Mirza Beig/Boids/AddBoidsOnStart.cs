using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddBoidsOnStart : MonoBehaviour
{
    Boids2D_Simulator simulator;
    public int boids = 100;

    void Start()
    {
        simulator = GetComponent<Boids2D_Simulator>();
        simulator.AddBoids(boids);
    }

    void Update()
    {
        
    }
}
