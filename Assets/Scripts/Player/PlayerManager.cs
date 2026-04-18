using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.UIElements;

public class PlayerManager : Singleton<PlayerManager>
{
    public GameObject interactionSetup;
    private GameObject changeSceneObj;
    private GameObject menuObj;
    private ComponentRaycast pistoletLogic;
    private GameObject xrOriginObj;

    private void Start()
    {
        changeSceneObj = GetComponentInChildren<LevelTransition>(true).gameObject;
        menuObj = GetComponentInChildren<MenuToggle>(true).gameObject;
        xrOriginObj = GetComponentInChildren<XROrigin>().gameObject;
        pistoletLogic = GetComponentInChildren<ComponentRaycast>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "Start") {
            menuObj.SetActive(false);
            StartCoroutine(ChangeToMainMenu());
        }
        if (currentScene.name == "CreativeMode" || currentScene.name == "RestrictionMode")
        {
            pistoletLogic.EnableDeleteButton();
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "Start" || scene.name == "MainMenu")
        {
            menuObj.SetActive(false);
        } else {
            menuObj.SetActive(true);
        }
        if (scene.name == "CreativeMode" || scene.name == "RestrictionMode")
        {
            pistoletLogic.EnableDeleteButton();
        } else {
            pistoletLogic.DisableDeleteButton();
        }
        Vector3 pos = Vector3.zero;
        xrOriginObj.transform.position = pos;
    }

    public void ChangeScene(int sceneIndex) {
        LevelTransition levelTransition = changeSceneObj.GetComponent<LevelTransition>();
        Animator animator = changeSceneObj.GetComponent<Animator>();
        levelTransition.scene = sceneIndex;
        animator.Play("ChangeSceneFade", 0, 0f);
    }

    private IEnumerator ChangeToMainMenu() {
        yield return new WaitForSeconds(0.4f);
        LevelTransition levelTransition = changeSceneObj.GetComponent<LevelTransition>();
        Animator animator = changeSceneObj.GetComponent<Animator>();
        levelTransition.scene = 1;
        animator.Play("ChangeSceneFade", 0, 0f);
    }
}
