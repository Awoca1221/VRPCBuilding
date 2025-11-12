using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance = null;
    public GameObject interactionSetup;
    private GameObject changeSceneObj;
    private GameObject menuObj;
    private GameObject xrOriginObj;

    private void Start()
    {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        changeSceneObj = GetComponentInChildren<LevelTransition>(true).gameObject;
        menuObj = GetComponentInChildren<MenuToggle>(true).gameObject;
        xrOriginObj = GetComponentInChildren<XROrigin>().gameObject;
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerManagerEvents playerManagerEvents = FindObjectOfType<PlayerManagerEvents>();
        if (playerManagerEvents != null) {
            playerManagerEvents.RegisterChangeScene(ChangeScene);
        } else {
            Debug.Log("PlayerManagerEvents отсутствует в стартовой сцене.");
        }
        if (SceneManager.GetActiveScene().name == "Start") {
            menuObj.SetActive(false);
            StartCoroutine(ChangeToMainMenu());
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "Start" || scene.name == "MainMenu") {
            menuObj.SetActive(false);
        } else {
            menuObj.SetActive(true);
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
