using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public enum Hand
{
    Left,
    Right
}

public class HandInfo : MonoBehaviour
{
    public Hand hand = Hand.Left;
    private InputActionAsset inputActions;
    [field: NonSerialized] public InputAction activateAction { get; set; } = null;

    void Start()
    {
        inputActions = GameObject.Find("Input Action Manager").GetComponent<InputActionManager>().actionAssets[0];
        switch (hand)
        {
            case Hand.Left:
                activateAction = inputActions.FindActionMap("XRI LeftHand Interaction").FindAction("Activate");
                break;
            
            case Hand.Right:
                activateAction = inputActions.FindActionMap("XRI RightHand Interaction").FindAction("Activate");
                break;
        }
    }
}
