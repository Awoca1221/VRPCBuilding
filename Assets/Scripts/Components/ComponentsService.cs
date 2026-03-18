using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ComponentsService : Singleton<ComponentsService>
{
    public IReadOnlyDictionary<string, List<DeviceInfo>> Components => componentPrefabs;
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
    private readonly Dictionary<string, List<DeviceInfo>> componentPrefabs = new();

    protected override void Awake()
    {
        base.Awake();

        foreach (var folder in componentFolders)
        {
            // Загружаем все префабы из папки Resources/ComponentsInfo/[folder]
            DeviceInfo[] prefabs = Resources.LoadAll<DeviceInfo>($"ComponentsInfo/{folder}");
            
            if (prefabs.Length > 0)
            {
                componentPrefabs[folder] = new List<DeviceInfo>(prefabs);
                // Debug.Log($"Loaded {prefabs.Length} prefabs from {folder}");
            }
            else
            {
                // Debug.LogWarning($"No prefabs found in {folder}");
            }
        }
    }

    public static async Task<GameObject> SpawnComponent(AssetReferenceGameObject prefab, Vector3 position)
    {
        return await Addressables.InstantiateAsync(prefab, position, Quaternion.identity).Task;
    }

    public static async Task<GameObject> LoadComponent(AssetReferenceGameObject prefab)
    {
        if (prefab.OperationHandle.IsValid() && prefab.OperationHandle.IsDone)
        {
            if (prefab.OperationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                return (GameObject)prefab.OperationHandle.Result;
            }
        }
        return await prefab.LoadAssetAsync<GameObject>().Task;
    }
}
