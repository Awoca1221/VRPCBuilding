using System.Collections.Generic;
using UnityEngine;

public class ComponentsService : Singleton<ComponentsService>
{
    public IReadOnlyDictionary<string, List<GameObject>> Components => componentPrefabs;
    public static readonly Dictionary<ComponentType, string> keys = new()
    {
        {ComponentType.Cooler, "Cooler"},
        {ComponentType.CPU, "CPU"},
        {ComponentType.GPU, "GPU"},
        {ComponentType.RAM, "RAM"},
        {ComponentType.Motherboard, "Motherboard"},
        {ComponentType.PowerSupply, "PowerSupply"},
        {ComponentType.StorageDevice, "StorageDevice"},
    };
    
    private static readonly string[] componentFolders = {
        "Cooler",
        "CPU",
        "GPU",
        "Motherboard",
        "PowerSupply",
        "RAM",
        "StorageDevice"
    };
    private readonly Dictionary<string, List<GameObject>> componentPrefabs = new();

    protected override void Awake()
    {
        base.Awake();

        foreach (var folder in componentFolders)
        {
            // Загружаем все префабы из папки Resources/PCComponents/[folder]
            GameObject[] prefabs = Resources.LoadAll<GameObject>($"PCComponents/{folder}");
            
            if (prefabs.Length > 0)
            {
                componentPrefabs[folder] = new List<GameObject>(prefabs);
                // Debug.Log($"Loaded {prefabs.Length} prefabs from {folder}");
            }
            else
            {
                // Debug.LogWarning($"No prefabs found in {folder}");
            }
        }
    }
}
