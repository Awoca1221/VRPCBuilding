using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using UnityEngine.XR.Interaction.Toolkit;

public class MenuToggle : MonoBehaviour
{
    public InputActionReference menuToggle;
    private Canvas canvas;

    public Camera mainCam;

    void Start()
    {
        canvas = GetComponent<Canvas>();
        menuToggle.action.performed += OnMenuButtonPressed;
    }

    private void OnMenuButtonPressed(InputAction.CallbackContext context)
    {
        if (!gameObject.active) {
            return;
        }
        Transform menuTransform = GetComponent<Transform>();

        menuTransform.forward = mainCam.transform.forward;
        UnityEngine.Vector3 pos = mainCam.ScreenToWorldPoint(new UnityEngine.Vector3(mainCam.pixelWidth / 2f, mainCam.pixelHeight / 2f, 1f));
        
        menuTransform.position = pos;

        canvas.enabled = !canvas.enabled;
    }

    void OnDestroy()
    {
        menuToggle.action.performed -= OnMenuButtonPressed;
    }
}
