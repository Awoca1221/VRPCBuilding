using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AttachObjectDevice))]
public class CPUPasteState : MonoBehaviour
{
    public Renderer cpuMeshRenderer;
    public Material material;
    private Material startMaterial;
    [Tooltip("Индекс материала в MeshRenderer (-1 для отключения изменения материала)")]
    public int indexOfMaterial = -1;
    [Tooltip("Необходим если менять текстуру модели не вариант")]
    public MeshRenderer pasteMeshRenderer;
    public bool isActiveOnStart = false;
    public bool IsPasteActive { get; private set; } = false;
    private Material[] materials;
    private AttachObjectDevice attachObjectDevice;

    // Start is called before the first frame update
    void Start()
    {
        if (cpuMeshRenderer != null && indexOfMaterial >= 0)
        {
            materials = cpuMeshRenderer.materials;
            startMaterial = materials[indexOfMaterial];
        }
        
        attachObjectDevice = GetComponent<AttachObjectDevice>();
        if (isActiveOnStart) Activate();
    }

    public void Activate()
    {
        if (startMaterial != null)
        {
            materials[indexOfMaterial] = material;
            cpuMeshRenderer.materials = materials;
        }
        if (pasteMeshRenderer)
        {
            pasteMeshRenderer.enabled = true;
        }

        IsPasteActive = true;

        if (attachObjectDevice.PCBuildRef != null)
        {
            attachObjectDevice.PCBuildRef.UpdateOverallStatus();
        }
    }

    public void Deactivate()
    {
        if (startMaterial != null)
        {
            materials[indexOfMaterial] = startMaterial;
            cpuMeshRenderer.materials = materials;
        }
        if (pasteMeshRenderer)
        {
            pasteMeshRenderer.enabled = false;
        }

        IsPasteActive = false;

        if (attachObjectDevice.PCBuildRef != null)
        {
            attachObjectDevice.PCBuildRef.UpdateOverallStatus();
        }
    }

}
