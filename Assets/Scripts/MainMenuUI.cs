using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void ChangeSceneButton(int sceneID)
    {
        PlayerManager.Instance.ChangeScene(sceneID);
    }
}
