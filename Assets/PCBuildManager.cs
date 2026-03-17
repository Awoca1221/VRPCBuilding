using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes.Test;
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
    public Status PCStatus => currentStatus;
    public IReadOnlyCollection<GameObject> ConnectedDevices => connectedDevices;
    public UnityAction OnOverallStatusUpdated;

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
        connectedDevices.Remove(device);
        UpdateOverallStatus();
        //Debug.Log($"Отключено устройство: {device.name}. Всего: {connectedDevices.Count}");
    }

    private void UpdateOverallStatus()
    {
        var requiredTypes = Enum.GetValues(typeof(ComponentType))
            .Cast<ComponentType>()
            .Where(t => t != ComponentType.NotSelected);

        // Собираем все ConnectionPoint из всех подключенных устройств
        var allSecuredPoints = new HashSet<ComponentType>();
        ItemCommon cpuInfo = null;
        ItemCommon gpuInfo = null;
        ItemCommon powerSupplyInfo = null;
        foreach (var device in connectedDevices)
        {
            AttachObjectDevice.Status deviceStatus = device.GetComponent<AttachObjectDevice>().DeviceStatus;
            if (deviceStatus == AttachObjectDevice.Status.FullySecured)
            {
                ComponentType deviceType = device.GetComponent<ItemCommon>().ComponentType;
                allSecuredPoints.Add(deviceType);
            }
            ItemCommon elemInfo = device.GetComponent<ItemCommon>();
            switch (elemInfo.ComponentType)
            {
                case ComponentType.CPU:
                    cpuInfo = elemInfo;
                    break;
                case ComponentType.Cooler:
                    gpuInfo = elemInfo;
                    break;
                case ComponentType.RAM:
                    powerSupplyInfo = elemInfo;
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
            var devInfo = device.GetComponent<ItemCommon>();
            Device deviceSave = new()
            {
                type = devInfo.ComponentType.ToString(),
                name = devInfo.Name,
                slotID = device.GetComponent<AttachObjectDevice>().SlotID
            };
            newBuild.devices.Add(deviceSave);
        }

        var savedData = SaveService.Load<PlayerBuildsList>("player_builds");
        savedData.builds.Add(newBuild);
        SaveService.Save("player_builds", savedData);
    }

    public void SpawnBuild(Build build)
    {
        if (connectedDevices.Count != 0) return;

        var componentPrefabs = ComponentsService.Instance.Components;
        Dictionary<string, List<SpawnObjInfo>> instantiatedObjs = new();
        
        foreach (var device in build.devices)
        {
            GameObject foundObject = componentPrefabs[device.type].FirstOrDefault(obj =>
                obj.GetComponent<ItemCommon>().Name == device.name
            );
            if (foundObject != null)
            {   
                SpawnObjInfo objInfo = new(){
                    obj = Instantiate(foundObject), slotID = device.slotID
                };
                instantiatedObjs[device.type].Add(objInfo);
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
    }

    public uint GetCPUPerformance()
    {
        if (currentStatus == Status.NotWorking) return 0;

        ItemCommon cpuInfo = null;
        ItemCommon coolerInfo = null;
        ItemCommon ramInfo = null;
        foreach (var elem in connectedDevices)
        {
            ItemCommon elemInfo = elem.GetComponent<ItemCommon>();
            switch (elemInfo.ComponentType)
            {
                case ComponentType.CPU:
                    cpuInfo = elemInfo;
                    break;
                case ComponentType.Cooler:
                    coolerInfo = elemInfo;
                    break;
                case ComponentType.RAM:
                    ramInfo = elemInfo;
                    break;
                default:
                    break;
            }
            if (cpuInfo != null && coolerInfo != null && ramInfo != null) break;
        }
        
        uint performance = (uint)(cpuInfo.Performance + ramInfo.FrequencyMhz * 0.5f);
        if (cpuInfo.TDP > coolerInfo.TDPLimit)
        {
            performance = (uint)(performance * 0.8f);
        }

        return performance;
    }

    public uint GetGPUPerformance()
    {
        if (currentStatus == Status.NotWorking) return 0;

        ItemCommon gpuInfo = null;
        foreach (var elem in connectedDevices)
        {
            ItemCommon elemInfo = elem.GetComponent<ItemCommon>();
            if (elemInfo.ComponentType == ComponentType.GPU)
            {
                gpuInfo =  elemInfo;
                break;
            }
        }
        
        uint performance = gpuInfo.Performance;
        
        return performance;
    }

    public uint GetOverallPerformance()
    {
        if (currentStatus == Status.NotWorking) return 0;
        
        uint performance = GetCPUPerformance() + GetGPUPerformance();
        
        return performance;
    }
}
