using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VSyncOn : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LockFPS()
    {
        Application.targetFrameRate = 140;
    }
}
