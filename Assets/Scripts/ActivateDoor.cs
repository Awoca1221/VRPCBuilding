using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioManager))]
public class ActivateDoor : MonoBehaviour
{
    public Stage stage;
    public InputActionReference test;

    private AudioManager audioManager;
    private Animator animator;
    private bool isOpen = false;

    void Start()
    {
        audioManager = GetComponent<AudioManager>();
        animator = GetComponent<Animator>();
        if (stage)
            stage.completeTask.AddListener(onCompleteTask);
        if (test)
            test.action.performed += ActivateTest;
    }
    
    private void onCompleteTask()
    {
        if (!isOpen && stage.AreAllTasksCompleted())
        {
            OpenDoor();
        }
    }

    private void ActivateTest(InputAction.CallbackContext context)
    {
        animator.SetTrigger("Activate");
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            animator.SetTrigger("Activate");
            audioManager.PlayOpenDoorSound();
            isOpen = true;
        }
    }

    public void CloseDoor()
    {
        if (isOpen)
        {
            animator.SetTrigger("Activate");
            audioManager.PlayCloseDoorSound();
            isOpen = false;
        }
    }

    void OnDestroy()
    {
        if (test)
            test.action.performed -= ActivateTest;
    }
}
