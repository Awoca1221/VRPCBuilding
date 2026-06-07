#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

public static class ComponentsSpawnerEditor
{
    private const float Spacing = 0.4f;
    private const string RootObjectName = "All_Components_Grid";

    [MenuItem("Tools/Spawn All Components in Grid")]
    public static void SpawnAllComponentsInGrid()
    {
        ComponentsService service = Object.FindFirstObjectByType<ComponentsService>();
        if (service == null)
        {
            GameObject temp = new GameObject("Temp_ComponentsService");
            service = temp.AddComponent<ComponentsService>();
            service.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
            Debug.Log("ComponentsService временно создан. Рекомендуется добавить его в сцену постоянно.");
        }

        if (service.Components == null || service.Components.Count == 0)
        {
            Debug.LogError("ComponentsService пуст. Проверьте Resources/ComponentsInfo/...");
            return;
        }

        List<DeviceInfo> allDevices = new List<DeviceInfo>();
        foreach (var kvp in service.Components)
            if (kvp.Value != null)
                allDevices.AddRange(kvp.Value);

        if (allDevices.Count == 0)
        {
            Debug.LogWarning("Нет DeviceInfo для спавна.");
            return;
        }

        List<GameObject> validPrefabs = new List<GameObject>();
        foreach (DeviceInfo info in allDevices)
        {
            if (info.Prefab != null && info.Prefab.editorAsset is GameObject prefab)
                validPrefabs.Add(prefab);
            else
                Debug.LogWarning($"Пропущен '{info.Name}' – отсутствует или невалидный Prefab.");
        }

        if (validPrefabs.Count == 0)
        {
            Debug.LogError("Не найдено ни одного префаба.");
            return;
        }

        int total = validPrefabs.Count;
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(total));

        GameObject root = new GameObject(RootObjectName);
        Undo.RegisterCreatedObjectUndo(root, "Create Grid Root");

        for (int i = 0; i < total; i++)
        {
            int row = i / gridSize;
            int col = i % gridSize;
            Vector3 position = new Vector3(col * Spacing, 0f, row * Spacing);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(validPrefabs[i], root.transform);
            instance.transform.position = position;
            instance.name = validPrefabs[i].name;

            // Отключаем кинематику у всех Rigidbody (включая дочерние)
            Rigidbody[] rbs = instance.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rbs)
                rb.isKinematic = true;

            Undo.RegisterCreatedObjectUndo(instance, "Spawn Component");
        }

        Selection.activeGameObject = root;
        Debug.Log($"Заспавнено {total} объектов, сетка {gridSize}x{gridSize}.");
    }
}
#endif