using UnityEngine;
using UnityEngine.UI;

public class ObjectRotator : MonoBehaviour
{
    [SerializeField] private Transform objectToRotate;
    [Tooltip("Скорость вращения (градус/сек)")]
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private HoldButton leftButton;
    [SerializeField] private HoldButton rightButton;
    
    private void Update()
    {
        if (objectToRotate == null || leftButton == null || rightButton == null)
        {
            return;
        }

        float rotationY = 0f;

        if (leftButton.IsHolding == true)
        {
            rotationY -= rotationSpeed * Time.deltaTime;
        }
        if (rightButton.IsHolding == true)
        {
            rotationY += rotationSpeed * Time.deltaTime;
        }
        
        objectToRotate.Rotate(0, rotationY, 0, Space.Self);
    }
}