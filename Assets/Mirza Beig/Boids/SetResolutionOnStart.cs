using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetResolutionOnStart : MonoBehaviour
{
    public Vector2Int startResolution = new(1920, 1080);

    void Start()
    {
        // Only do this if the game is launched on Desktop.

        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Screen.SetResolution(startResolution.x, startResolution.y, false);
        }
    }

    void Update()
    {

    }
}
