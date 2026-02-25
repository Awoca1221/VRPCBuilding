using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PCStatus
{
    NotWorking, // Не проходит минимальные требования к работоспособности
    Working // ПК в рабочем состоянии
}

public class PCBuildManager : MonoBehaviour
{
    public PCStatus Status => currentStatus;
    public IReadOnlyCollection<GameObject> ConnectedDevices => connectedDevices;

    private HashSet<GameObject> connectedDevices = new();
    private PCStatus currentStatus = PCStatus.NotWorking;

    public void OnDeviceConnected(GameObject device)
    {
        connectedDevices.Add(device);
        UpdateOverallStatus();
        Debug.Log($"Подключено устройство: {device.name}. Всего: {connectedDevices.Count}");
    }

    public void OnDeviceDisconnected(GameObject device)
    {
        connectedDevices.Remove(device);
        UpdateOverallStatus();
        Debug.Log($"Отключено устройство: {device.name}. Всего: {connectedDevices.Count}");
    }

    private void UpdateOverallStatus()
    {
        var requiredTypes = System.Enum.GetValues(typeof(ComponentType))
            .Cast<ComponentType>()
            .Where(t => t != ComponentType.NotSelected);

        // Собираем все ConnectionPoint из всех подключенных устройств
        var allSecuredPoints = new HashSet<ComponentType>();
        
        foreach (var device in connectedDevices)
        {
            ComponentStatus deviceStatus = device.GetComponent<AttachObject>().CompStatus;
            if (deviceStatus == ComponentStatus.FullySecured)
            {
                ComponentType deviceType = device.GetComponent<ItemCommon>().ComponentType;
                allSecuredPoints.Add(deviceType);
            }
        }

        bool allTypesCovered = requiredTypes.All(reqType => allSecuredPoints.Contains(reqType));
        currentStatus = allTypesCovered ? PCStatus.Working : PCStatus.NotWorking;
    }
}
