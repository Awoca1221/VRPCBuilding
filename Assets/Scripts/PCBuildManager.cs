using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Device
{
    public string type = null;
    public string itemID = null;
    public uint slotID = 0;
}

[Serializable]
public class Build
{
    public List<Device> devices = new();
}

[Serializable]
public class PlayerBuildsList
{
    public List<Build> builds = new();
}

public class PCBuildManager : MonoBehaviour
{
    public enum Status
    {
        NotWorking, // Не проходит минимальные требования к работоспособности
        Unstable, // ПК в рабочем состоянии, но возможны проблемы в нагрузке
        Working // ПК в рабочем состоянии
    }

    public class CPUPerformance
    {
        public readonly uint performance;
        public readonly bool isOverheated;

        public CPUPerformance(uint p, bool io)
        {
            performance = p;
            isOverheated = io;
        }
    }

    [Tooltip("Область для проверки возможности спавна билда")]
    public Collider testEmptySpace;
    public Status PCStatus => currentStatus;
    public bool IsNotEnoughPower { get; private set; } = false;
    public bool IsWithoutCPUPaste { get; private set; } = false;
    public IReadOnlyCollection<GameObject> ConnectedDevices => connectedDevices;
    public int CountConnectedDevices => connectedDevices.Count();
    public UnityAction OnOverallStatusUpdated;
    public UnityEvent OnWorkingStatusEvent;

    private class SpawnObjInfo
    {
        public GameObject obj;
        public uint slotID;
    }
    private readonly HashSet<GameObject> connectedDevices = new();
    private Status currentStatus = Status.NotWorking;

    public void OnDeviceConnected(GameObject device)
    {
        connectedDevices.Add(device);
        UpdateOverallStatus();
        //Debug.Log($"Подключено устройство: {device.name}. Всего: {connectedDevices.Count}");
    }

    public void OnDeviceDisconnected(GameObject device)
    {
        if (device != null)
        {
            connectedDevices.Remove(device);
        }
        else
        {
            connectedDevices.RemoveWhere(item => item == null);
        }
        UpdateOverallStatus();
        //Debug.Log($"Отключено устройство: {device.name}. Всего: {connectedDevices.Count}");
    }

    public bool IsSpawnAreaClear()
    {
        if (testEmptySpace == null)
        {
            Debug.Log("testEmptySpace не выставлен");
            return true;
        }

        Collider[] hitColliders = Physics.OverlapBox(
            testEmptySpace.transform.position,
            testEmptySpace.bounds.extents,
            testEmptySpace.transform.rotation,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore
        );

        // если есть хоть один коллайдер, который не дочерний к этому объекту
        return !hitColliders.Any(col => !col.transform.IsChildOf(transform));
    }

    public void UpdateOverallStatus()
    {
        var requiredTypes = Enum.GetValues(typeof(ComponentType))
            .Cast<ComponentType>()
            .Where(t => t != ComponentType.NotSelected);

        // Собираем все FullySecured устройства
        var allSecuredPoints = new HashSet<ComponentType>();
        CPUInfo cpuInfo = null;
        CPUPasteState pasteState = null;
        GPUInfo gpuInfo = null;
        PowerSupplyInfo powerSupplyInfo = null;
        foreach (var device in connectedDevices)
        {
            AttachObjectDevice.Status deviceStatus = device.GetComponent<AttachObjectDevice>().DeviceStatus;
            if (deviceStatus == AttachObjectDevice.Status.FullySecured)
            {
                ComponentType deviceType = device.GetComponent<AttachObjectDevice>().deviceInfo.ComponentType;
                allSecuredPoints.Add(deviceType);
            }
            DeviceInfo elemInfo = device.GetComponent<AttachObjectDevice>().deviceInfo;
            switch (elemInfo)
            {
                case CPUInfo:
                    cpuInfo = (CPUInfo)elemInfo;
                    pasteState = device.GetComponent<CPUPasteState>();
                    break;
                case GPUInfo:
                    gpuInfo = (GPUInfo)elemInfo;
                    break;
                case PowerSupplyInfo:
                    powerSupplyInfo = (PowerSupplyInfo)elemInfo;
                    break;
                default:
                    break;
            }
        }

        bool allTypesCovered = requiredTypes.All(reqType => allSecuredPoints.Contains(reqType));
        if (!allTypesCovered)
        {
            currentStatus = Status.NotWorking;
            OnOverallStatusUpdated?.Invoke();
            return;
        }

        IsNotEnoughPower = (cpuInfo.TDP + gpuInfo.TDP + 100) > powerSupplyInfo.PowerSupplyMaxPower;
        IsWithoutCPUPaste = !pasteState.IsPasteActive;
        if (IsNotEnoughPower || IsWithoutCPUPaste)
        {
            currentStatus = Status.Unstable;
        }
        else
        {
            currentStatus = Status.Working;
            OnWorkingStatusEvent?.Invoke();
        }
        
        OnOverallStatusUpdated?.Invoke();
    }

    public void SaveBuild()
    {
        if (currentStatus != Status.Working) return;
        
        Build newBuild = new();
        foreach (var device in connectedDevices)
        {
            var attachObject = device.GetComponent<AttachObjectDevice>();
            var devInfo = attachObject.deviceInfo;
            Device deviceSave = new()
            {
                type = devInfo.ComponentType.ToString(),
                itemID = devInfo.ItemID,
                slotID = attachObject.SlotID
            };
            newBuild.devices.Add(deviceSave);
        }

        newBuild.devices = newBuild.devices
        .OrderBy(d => d.type)
        .ToList();

        var savedData = SaveService.Load<PlayerBuildsList>("player_builds");
        savedData.builds.Add(newBuild);
        SaveService.Save("player_builds", savedData);
    }

    public async void SpawnBuild(Build build, UnityAction callback = null)
    {
        if (connectedDevices.Count != 0) return;
        var componentPrefabs = ComponentsService.Instance.Components;
        Dictionary<string, List<SpawnObjInfo>> instantiatedObjs = new();
        
        foreach (var device in build.devices)
        {
            DeviceInfo foundObject = componentPrefabs[device.type].FirstOrDefault(obj =>
                obj.ItemID == device.itemID
            );
            if (foundObject != null)
            {   
                SpawnObjInfo objInfo = new(){
                    obj = await ComponentsService.SpawnComponent(foundObject.Prefab, Vector3.down), slotID = device.slotID
                };
                if (!instantiatedObjs.TryGetValue(device.type, out var list))
                {
                    list = new List<SpawnObjInfo>();
                    instantiatedObjs[device.type] = list;
                }

                list.Add(objInfo);
            }
        }
        instantiatedObjs["PowerSupply"][0].obj.GetComponent<AttachObjectDevice>().ForceAttachAndSetup(gameObject);
        instantiatedObjs.Remove("PowerSupply");
        instantiatedObjs["Motherboard"][0].obj.GetComponent<AttachObjectDevice>().ForceAttachAndSetup(gameObject);
        GameObject motherboard = instantiatedObjs["Motherboard"][0].obj;
        instantiatedObjs.Remove("Motherboard");

        foreach (var devices in instantiatedObjs)
        {
            switch (devices.Key)
            {
                case "Cooler":
                case "GPU":
                case "RAM":
                    foreach (var device in devices.Value)
                    {
                        device.obj.GetComponent<AttachObjectDevice>().ForceAttachAndSetup(motherboard, device.slotID);
                    }
                    break;
                case "CPU":
                    foreach (var device in devices.Value)
                    {
                        device.obj.GetComponent<AttachObjectDevice>().ForceAttachAndSetup(motherboard, device.slotID);
                        if (device.obj.TryGetComponent<CPUPasteState>(out var pasteState))
                        {
                            pasteState.Activate();
                        }
                    }
                    break;
                case "StorageDevice":
                    foreach (var device in devices.Value)
                    {
                        device.obj.GetComponent<AttachObjectDevice>().ForceAttachAndSetup(gameObject, device.slotID);
                    }
                    break;
                case null:
                    break;
            }
        }
        callback?.Invoke();
    }

    public uint GetPrice()
    {
        uint price = 0;
        foreach (var elem in connectedDevices)
        {
            price += elem.GetComponent<AttachObjectDevice>().deviceInfo.Price;
        }
        
        return price;
    }

    public CPUPerformance GetCPUPerformance()
    {
        if (currentStatus == Status.NotWorking)
        {
            return new CPUPerformance(0, false);
        }

        CPUInfo cpuInfo = null;
        CoolerInfo coolerInfo = null;
        List<RAMInfo> ramInfo = null;
        foreach (var elem in connectedDevices)
        {
            DeviceInfo elemInfo = elem.GetComponent<AttachObjectDevice>().deviceInfo;
            switch (elemInfo)
            {
                case CPUInfo:
                    cpuInfo = (CPUInfo)elemInfo;
                    break;
                case CoolerInfo:
                    coolerInfo = (CoolerInfo)elemInfo;
                    break;
                case RAMInfo:
                    ramInfo.Add((RAMInfo)elemInfo);
                    break;
            }
        }
        
        uint ramFrequency = ramInfo[0].FrequencyMhz;
        foreach (var ram in ramInfo)
        {
            if (ram.FrequencyMhz < ramFrequency)
            {
                ramFrequency = ram.FrequencyMhz;
            }
        }

        uint performance = (uint)(cpuInfo.Performance + ramFrequency * 0.5f);
        bool isOverheated = false;
        if (cpuInfo.TDP > coolerInfo.TDPLimit)
        {
            float overheatFactor = Math.Max((float)coolerInfo.TDPLimit / cpuInfo.TDP, 0.3f);
            performance = (uint)(performance * overheatFactor);
            isOverheated = true;
        }

        return new CPUPerformance(performance, isOverheated);
    }

    public uint GetGPUPerformance()
    {
        if (currentStatus == Status.NotWorking) return 0;

        GPUInfo gpuInfo = null;
        foreach (var elem in connectedDevices)
        {
            DeviceInfo elemInfo = elem.GetComponent<AttachObjectDevice>().deviceInfo;
            if (elemInfo is GPUInfo info)
            {
                gpuInfo =  info;
                break;
            }
        }
        
        float adjustmentMult = 1.2f;
        uint performance = (uint)(gpuInfo.Performance * adjustmentMult);
        
        return performance;
    }

    public uint GetOverallPerformance()
    {
        if (currentStatus == Status.NotWorking) return 0;

        GPUInfo gpuInfo = null;
        List<RAMInfo> ramInfo = null;
        foreach (var elem in connectedDevices)
        {
            DeviceInfo elemInfo = elem.GetComponent<AttachObjectDevice>().deviceInfo;
            switch (elemInfo)
            {
                case GPUInfo:
                    gpuInfo = (GPUInfo)elemInfo;
                    break;
                case RAMInfo:
                    ramInfo.Add((RAMInfo)elemInfo);
                    break;
            }
        }

        uint memoryMultiplier = 500;
        uint performance = GetCPUPerformance().performance + GetGPUPerformance() + gpuInfo.MemoryAmountGB * memoryMultiplier;
        foreach (var ram in ramInfo)
        {
            performance += ram.MemoryAmountGB * memoryMultiplier;
        }
        
        return performance;
    }
}
