using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids2D_SetBoundsToAspectRatio : MonoBehaviour
{
    Boids2D_Simulator simulator;

    void Start()
    {
        simulator = GetComponent<Boids2D_Simulator>();

        float aspectRatio = Screen.width / (float)Screen.height;
        simulator.bounds = new Vector2(aspectRatio, 1);
    }

    void Update()
    {

    }
}
