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
    public string name = null;
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
        public readonly bool isWithoutPaste;

        public CPUPerformance(uint p, bool io, bool iwp)
        {
            performance = p;
            isOverheated = io;
            isWithoutPaste = iwp;
        }
    }

    [Tooltip("Область для проверки возможности спавна билда")]
    public Collider testEmptySpace;
    public Status PCStatus => currentStatus;
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

        // Собираем все ConnectionPoint из всех подключенных устройств
        var allSecuredPoints = new HashSet<ComponentType>();
        CPUInfo2 cpuInfo = null;
        GPUInfo2 gpuInfo = null;
        PowerSupplyInfo2 powerSupplyInfo = null;
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
                case CPUInfo2:
                    cpuInfo = (CPUInfo2)elemInfo;
                    break;
                case GPUInfo2:
                    gpuInfo = (GPUInfo2)elemInfo;
                    break;
                case PowerSupplyInfo2:
                    powerSupplyInfo = (PowerSupplyInfo2)elemInfo;
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

        bool hasEnoughPower = (cpuInfo.TDP + gpuInfo.TDP + 100) <= powerSupplyInfo.PowerSupplyMaxPower;
        if (hasEnoughPower)
        {
            currentStatus = Status.Working;
            OnWorkingStatusEvent?.Invoke();
        }
        else
        {
            currentStatus = Status.Unstable;
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
                name = devInfo.Name,
                slotID = attachObject.SlotID
            };
            newBuild.devices.Add(deviceSave);
        }

        newBuild.devices = newBuild.devices
        .OrderBy(d => d.type)
        .ToList();

        var savedData = SaveService.Load<PlayerBuildsList>("player_builds") ?? new();
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
                obj.Name == device.name
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
        instantiatedObjs["PowerSupply"][0].obj.GetComponent<AttachObjectDevice>().ForceAttach(gameObject);
        instantiatedObjs.Remove("PowerSupply");
        instantiatedObjs["Motherboard"][0].obj.GetComponent<AttachObjectDevice>().ForceAttach(gameObject);
        GameObject motherboard = instantiatedObjs["Motherboard"][0].obj;
        instantiatedObjs.Remove("Motherboard");

        foreach (var devices in instantiatedObjs)
        {
            switch (devices.Key)
            {
                case "Cooler":
                case "CPU":
                case "GPU":
                case "RAM":
                    foreach (var device in devices.Value)
                    {
                        device.obj.GetComponent<AttachObjectDevice>().ForceAttach(motherboard, device.slotID);
                    }
                    break;
                case "StorageDevice":
                    foreach (var device in devices.Value)
                    {
                        device.obj.GetComponent<AttachObjectDevice>().ForceAttach(gameObject, device.slotID);
                    }
                    break;
                case null:
                    break;
            }
        }
        callback?.Invoke();
    }

    public CPUPerformance GetCPUPerformance()
    {
        if (currentStatus == Status.NotWorking)
        {
            return new CPUPerformance(0, false, false);
        }

        CPUInfo2 cpuInfo = null;
        CoolerInfo2 coolerInfo = null;
        RAMInfo2 ramInfo = null;
        foreach (var elem in connectedDevices)
        {
            DeviceInfo elemInfo = elem.GetComponent<AttachObjectDevice>().deviceInfo;
            switch (elemInfo)
            {
                case CPUInfo2:
                    cpuInfo = (CPUInfo2)elemInfo;
                    break;
                case CoolerInfo2:
                    coolerInfo = (CoolerInfo2)elemInfo;
                    break;
                case RAMInfo2:
                    ramInfo = (RAMInfo2)elemInfo;
                    break;
                default:
                    break;
            }
            if (cpuInfo != null && coolerInfo != null && ramInfo != null) break;
        }
        
        uint performance = (uint)(cpuInfo.Performance + ramInfo.FrequencyMhz * 0.5f);
        bool isOverheated = false;
        bool isWithoutPaste = false;
        if (cpuInfo.TDP > coolerInfo.TDPLimit)
        {
            performance = (uint)(performance * 0.8f);
            isOverheated = true;
        }
        GameObject cpu = cpuInfo.GameObject();
        if (!cpu.TryGetComponent<ChangeMaterial>(out var changeMaterial))
        {
            changeMaterial = cpu.GetComponentInChildren<ChangeMaterial>();
        }
        if (changeMaterial != null && !changeMaterial.changed)
        {
            performance = (uint)(performance * 0.5f);
            isWithoutPaste = true;
        }

        return new CPUPerformance(performance, isOverheated, isWithoutPaste);
    }

    public uint GetGPUPerformance()
    {
        if (currentStatus == Status.NotWorking) return 0;

        GPUInfo2 gpuInfo = null;
        foreach (var elem in connectedDevices)
        {
            DeviceInfo elemInfo = elem.GetComponent<AttachObjectDevice>().deviceInfo;
            if (elemInfo is GPUInfo2 info)
            {
                gpuInfo =  info;
                break;
            }
        }
        
        uint performance = gpuInfo.Performance;
        
        return performance;
    }

    public uint GetOverallPerformance()
    {
        if (currentStatus == Status.NotWorking) return 0;
        
        uint performance = GetCPUPerformance().performance + GetGPUPerformance();
        
        return performance;
    }
}
