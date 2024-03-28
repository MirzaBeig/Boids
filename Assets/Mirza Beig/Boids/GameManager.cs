using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Vector2Int startResolution = new(1920, 1080);

    [Space]

    public int boindsOnStart = 100;

    void Start()
    {
        // Only do this if the game is launched on Desktop.

        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Screen.SetResolution(startResolution.x, startResolution.y, false);
        }

        FindAnyObjectByType<Boids2D_Simulator>().AddBoids(boindsOnStart);
    }

    void Update()
    {

    }
}
