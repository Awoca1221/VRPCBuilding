using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PCBuildManager : MonoBehaviour
{
    public enum Status
    {
        NotWorking, // Не проходит минимальные требования к работоспособности
        Working // ПК в рабочем состоянии
    }
    public Status PCStatus => currentStatus;
    public IReadOnlyCollection<GameObject> ConnectedDevices => connectedDevices;

    private HashSet<GameObject> connectedDevices = new();
    private Status currentStatus = Status.NotWorking;

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
            AttachObjectDevice.Status deviceStatus = device.GetComponent<AttachObjectDevice>().DeviceStatus;
            if (deviceStatus == AttachObjectDevice.Status.FullySecured)
            {
                ComponentType deviceType = device.GetComponent<ItemCommon>().ComponentType;
                allSecuredPoints.Add(deviceType);
            }
        }

        bool allTypesCovered = requiredTypes.All(reqType => allSecuredPoints.Contains(reqType));
        currentStatus = allTypesCovered ? Status.Working : Status.NotWorking;
    }
}
