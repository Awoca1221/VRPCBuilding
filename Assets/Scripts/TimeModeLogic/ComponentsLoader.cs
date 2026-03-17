using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class ComponentsLoader : MonoBehaviour
{
    private IReadOnlyDictionary<string, List<GameObject>> componentPrefabs;
    private List<GameObject> spawnedObjs = new();
    public TaskManager taskManager;
    public CheckMultipleConnections ramCheck;
    public bool loadOnStart = true;

    void Start()
    {
        componentPrefabs = ComponentsService.Instance.Components;
    }

    // Спавн случайных компонентов в заданной области
    private void SpawnRandomComponents()
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
                GameObject randomPrefab = category.Value[Random.Range(0, category.Value.Count)];
                
                // Создаем экземпляр префаба
                Instantiate(randomPrefab, currentPosition, Quaternion.identity);

                currentPosition.x += spacing;
                if (currentPosition.x > spawnAreaCenter.x + SpawnAreaSize.x / 2)
                {
                    currentPosition.x = spawnAreaCenter.x - SpawnAreaSize.x / 2 + spacing;
                    currentPosition.z -= spacing;
                }
            }
        }
    }

    public void SpawnFullBuildWrapper()
    {
        StartCoroutine(SpawnFullBuild());
    }

    private IEnumerator SpawnFullBuild()
    {
        Dictionary<GameObject, string> build = new();
        List<GameObject> uncomp = new();

        // Берём за основу случайную материнскую плату
        GameObject motherboard = componentPrefabs["Motherboard"][Random.Range(0, componentPrefabs["Motherboard"].Count)];
        MotherboardInfo motherboardInfo = motherboard.GetComponent<ItemCommon>().GetMotherboardInfo();
        build.Add(motherboard, "5");
        // uint TDPLimit = 0;
        // uint PowerSupplyMaxPower = 0;
        List<GameObject> compatibleObjs = new();
        List<GameObject> uncompatibleObjs = new();

        // Выбираем процессор
        // compatibleObjs = componentPrefabs["CPU"].Where(c => c.GetComponent<ItemCommon>().GetCPUInfo().SocketType == motherboardInfo.SocketType).ToList();
        foreach (GameObject obj in componentPrefabs["CPU"])
        {
            CPUInfo info = obj.GetComponent<ItemCommon>().GetCPUInfo();
            if (info.SocketType == motherboardInfo.SocketType)
                compatibleObjs.Add(obj);
            else
                uncompatibleObjs.Add(obj);
        }

        if (compatibleObjs.Count > 0)
            build.Add(compatibleObjs[Random.Range(0, compatibleObjs.Count)], "1");
        if (uncompatibleObjs.Count > 0)
            uncomp.Add(uncompatibleObjs[Random.Range(0, uncompatibleObjs.Count)]);
        compatibleObjs.Clear();

        // Выбираем оперативную память
        compatibleObjs = componentPrefabs["RAM"].Where(c => c.GetComponent<ItemCommon>().GetRAMInfo().DDRType == motherboardInfo.DDRType).ToList();
        if (compatibleObjs.Count > 0)
            build.Add(compatibleObjs[Random.Range(0, compatibleObjs.Count)], "ram");
        compatibleObjs.Clear();
        
        // Выбираем кулер
        compatibleObjs = componentPrefabs["Cooler"].Where(c => c.GetComponent<ItemCommon>().GetCoolerInfo().SupportSockets.Contains(motherboardInfo.SocketType)).ToList();
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
                GameObject uncompObj = uncomp[0];
                createdObj = Instantiate(uncompObj, currentPosition, Quaternion.identity);
                yield return null;
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
                    createdObj = Instantiate(component.Key, currentPosition, Quaternion.identity);
                    yield return null;
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
                createdObj = Instantiate(component.Key, currentPosition, Quaternion.identity);
                yield return null;
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
