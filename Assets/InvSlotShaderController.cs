using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvSlotShaderController : MonoBehaviour
{
    private Material material;
    [SerializeField] private string propertyName = "_FresnelPower";

    void Start()
    {
        // Берем материал у текущего объекта
        material = GetComponent<Renderer>().material;
    }

    // Вызываем через UnityEvent
    public void SetFresnelPower(float value)
    {
        if (material != null)
        {
            material.SetFloat(propertyName, value);
        }
    }
}
