using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using UnityEditor.MemoryProfiler;
using System.Net.WebSockets;

public enum ComponentStatus
{
    NotInserted,     // Не вставлен
    InsertedLoose,   // Вставлен, но не закреплён
    FullySecured     // Полностью подключён (все винты)
}

public class ConnectionPoint : MonoBehaviour
{
    [Header("Con-point ID (object name if empty)")]
    public string pointID = "";
    public ComponentType connectionType = ComponentType.NotSelected;
    
    public UnityAction<string, ConnectionPoint> onStatusChanged;
    public ComponentStatus Status => currentStatus;
    public bool IsFullyConnected => currentStatus == ComponentStatus.FullySecured;
    public ConnectionPoint[] GetSubPoints => subPoints;
    
    private ConnectionPoint[] subPoints;
    private ScrewPoint[] requiredScrews; // Винты для этого слота (0 = не нужны)
    private ComponentStatus currentStatus = ComponentStatus.NotInserted;
    
    void Start()
    {
        if (pointID.Length == 0) pointID = name;
    }
    
    public void OnConnect(GameObject PCComponent)
    {
        subPoints = PCComponent.GetComponentsInChildren<ConnectionPoint>();
        requiredScrews = PCComponent.GetComponentsInChildren<ScrewPoint>();
        foreach (var sub in subPoints) sub.onStatusChanged += OnSubStatusChanged;
        foreach (var screw in requiredScrews) screw.onStatusChanged += CheckScrews;
        currentStatus = ComponentStatus.InsertedLoose;
        onStatusChanged?.Invoke(pointID, this);
    }
    
    public void OnDisconnect()
    {
        foreach (var sub in subPoints) sub.onStatusChanged -= OnSubStatusChanged;
        foreach (var screw in requiredScrews) screw.onStatusChanged -= CheckScrews;
        subPoints = null;
        requiredScrews = null;
        currentStatus = ComponentStatus.NotInserted;
        onStatusChanged?.Invoke(pointID, this);
    }

    private void OnSubStatusChanged(string subPointID, ConnectionPoint subPoint)
    {
        onStatusChanged?.Invoke(subPointID, subPoint);
    }
    
    private void CheckScrews()
    {
        UpdateStatus();
    }
    
    private void UpdateStatus()
    {
        ComponentStatus lastStatus = currentStatus;
        if (requiredScrews.Length == 0 || requiredScrews.All(s => s.IsSecured)) 
        {
            currentStatus = ComponentStatus.FullySecured;
        } 
        else 
        {
            currentStatus = ComponentStatus.InsertedLoose;
        }
        if (lastStatus != currentStatus)
        {
            onStatusChanged?.Invoke(pointID, this);
        }
    }
    
    void OnDestroy()
    {
        if (subPoints != null) foreach (var sub in subPoints) sub.onStatusChanged -= OnSubStatusChanged;
        if (requiredScrews != null) foreach (var screw in requiredScrews) screw.onStatusChanged -= CheckScrews;
    }
}
