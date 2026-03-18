using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Renderer))]
public class ComponentsLoader : MonoBehaviour
{
    private IReadOnlyDictionary<string, List<DeviceInfo>> componentPrefabs;
    private List<GameObject> spawnedObjs = new();
    public TaskManager taskManager;
    public CheckMultipleConnections ramCheck;
    public bool loadOnStart = true;

    public UnityEvent onObjectsSpawned = null;

    void Start()
    {
        componentPrefabs = ComponentsService.Instance.Components;
    }

    // Спавн случайных компонентов в заданной области
    private async void SpawnRandomComponents()
    {
        Renderer cubeRenderer = GetComponent<Renderer>();
        Vector3 spawnAreaCenter = cubeRenderer.bounds.center;
        Vector3 SpawnAreaSize = cubeRenderer.bounds.size;
        float spacing = 0.25f; // расстояние между компонентами
        Vector3 currentPosition = spawnAreaCenter - SpawnAreaSize / 2 + Vector3.one * spacing;
        foreach (var category in componentPrefabs)
        {
            if (category.Value.Count > 0)
            {
                // Выбираем случайный префаб из категории
                DeviceInfo randomPrefab = category.Value[Random.Range(0, category.Value.Count)];
                
                // Создаем экземпляр префаба
                await ComponentsService.SpawnComponent(randomPrefab.Prefab, currentPosition);

                currentPosition.x += spacing;
                if (currentPosition.x > spawnAreaCenter.x + SpawnAreaSize.x / 2)
                {
                    currentPosition.x = spawnAreaCenter.x - SpawnAreaSize.x / 2 + spacing;
                    currentPosition.z -= spacing;
                }
            }
        }
    }
    /*
    public async void SpawnFullBuildWrapper()
    {
        //StartCoroutine(SpawnFullBuild());

    }
    */

    public async void SpawnFullBuild()
    {
        Dictionary<DeviceInfo, string> build = new();
        List<DeviceInfo> uncomp = new();

        // Берём за основу случайную материнскую плату
        MotherboardInfo2 motherboard = (MotherboardInfo2)componentPrefabs["Motherboard"][Random.Range(0, componentPrefabs["Motherboard"].Count)];
        build.Add(motherboard, "5");
        // uint TDPLimit = 0;
        // uint PowerSupplyMaxPower = 0;
        List<DeviceInfo> compatibleObjs = new();
        List<DeviceInfo> uncompatibleObjs = new();

        // Выбираем процессор
        // compatibleObjs = componentPrefabs["CPU"].Where(c => c.GetComponent<ItemCommon>().GetCPUInfo().SocketType == motherboardInfo.SocketType).ToList();
        foreach (CPUInfo2 info in componentPrefabs["CPU"].Cast<CPUInfo2>())
        {
            if (info.SocketType == motherboard.SocketType)
                compatibleObjs.Add(info);
            else
                uncompatibleObjs.Add(info);
        }

        if (compatibleObjs.Count > 0)
            build.Add(compatibleObjs[Random.Range(0, compatibleObjs.Count)], "1");
        if (uncompatibleObjs.Count > 0)
            uncomp.Add(uncompatibleObjs[Random.Range(0, uncompatibleObjs.Count)]);
        compatibleObjs.Clear();
        uncompatibleObjs.Clear();

        // Выбираем оперативную память
        compatibleObjs.AddRange(componentPrefabs["RAM"].Cast<RAMInfo2>().Where(c => c.DDRType == motherboard.DDRType).ToList());
        if (compatibleObjs.Count > 0)
            build.Add(compatibleObjs[Random.Range(0, compatibleObjs.Count)], "ram");
        compatibleObjs.Clear();
        
        // Выбираем кулер
        compatibleObjs.AddRange(componentPrefabs["Cooler"].Cast<CoolerInfo2>().Where(c => c.SupportSockets.Contains(motherboard.SocketType)).ToList());
        if (compatibleObjs.Count > 0)
            build.Add(compatibleObjs[Random.Range(0, compatibleObjs.Count)], "3");
        compatibleObjs.Clear();

        // Выбираем видеокарту
        build.Add(componentPrefabs["GPU"][Random.Range(0, componentPrefabs["GPU"].Count)], "6");

        // Выбираем накопитель
        build.Add(componentPrefabs["StorageDevice"][Random.Range(0, componentPrefabs["StorageDevice"].Count)], "7");

        // Выбираем блок питания
        build.Add(componentPrefabs["PowerSupply"][Random.Range(0, componentPrefabs["PowerSupply"].Count)], "8");

        // Спавним комплектующие
        Renderer cubeRenderer = GetComponent<Renderer>();
        Vector3 spawnAreaCenter = cubeRenderer.bounds.center;
        Vector3 SpawnAreaSize = cubeRenderer.bounds.size;
        float spacing = 0.2f; // расстояние между компонентами
        Vector3 currentPosition = spawnAreaCenter - SpawnAreaSize / 2 + Vector3.one * spacing;
        GameObject createdObj;

        foreach (var component in build)
        {
            // Добавление несовместимых частей (пока только процессор)
            if (uncomp.Count != 0 && Random.Range(0f, 1f) < 0.25f)
            {
                DeviceInfo uncompObj = uncomp[0];
                createdObj = await ComponentsService.SpawnComponent(uncompObj.Prefab, currentPosition);
                //yield return null;
                spawnedObjs.Add(createdObj);
                uncomp.Remove(uncompObj);
                currentPosition.x += spacing;
                if (currentPosition.x > spawnAreaCenter.x + SpawnAreaSize.x / 2)
                {
                    currentPosition.x = spawnAreaCenter.x - SpawnAreaSize.x / 2 + spacing;
                    currentPosition.z -= spacing;
                }
            }

            // Создаем экземпляр префаба
            if (component.Value == "ram")
            {   
                ramCheck.Objects.Clear();
                for (int i = 0; i < 2; i++)
                {
                    createdObj = await ComponentsService.SpawnComponent(component.Key.Prefab, currentPosition);
                    //yield return null;
                    createdObj.GetComponent<AttachObjectDevice>().MultipleConnections = ramCheck;
                    ramCheck.Objects.Add(createdObj);
                    spawnedObjs.Add(createdObj);

                    currentPosition.x += spacing;
                    if (currentPosition.x > spawnAreaCenter.x + SpawnAreaSize.x / 2)
                    {
                        currentPosition.x = spawnAreaCenter.x - SpawnAreaSize.x / 2 + spacing;
                        currentPosition.z -= spacing;
                    }
                }
                ramCheck.Restart();
            }
            else if (component.Value != "")
            {
                createdObj = await ComponentsService.SpawnComponent(component.Key.Prefab, currentPosition);
                //yield return null;
                createdObj.GetComponent<AttachObjectDevice>().OnConnectEvents.AddListener(() => {taskManager.CompleteTask(component.Value);});
                createdObj.GetComponent<AttachObjectDevice>().OnDisconnectEvents.AddListener(() => {taskManager.UncompleteTask(component.Value);});
                spawnedObjs.Add(createdObj);
                
                currentPosition.x += spacing;
                if (currentPosition.x > spawnAreaCenter.x + SpawnAreaSize.x / 2)
                {
                    currentPosition.x = spawnAreaCenter.x - SpawnAreaSize.x / 2 + spacing;
                    currentPosition.z -= spacing;
                }
            }
        }

        onObjectsSpawned?.Invoke();
    }

    public void UnloadBuild()
    {
        ramCheck.Objects.Clear();
        foreach(var obj in spawnedObjs)
        {
            obj.GetComponent<AttachObjectDevice>().OnDisconnectEvents.Invoke();
            Destroy(obj);
        }
        for (int i = 0; i < 10; i++)
        {
            taskManager.UncompleteTask((i+1).ToString());
        }
        spawnedObjs.Clear();
    }
}
