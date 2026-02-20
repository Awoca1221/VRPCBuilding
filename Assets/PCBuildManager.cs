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
    [Header("Con-points (auto setup if empty)")]
    public ConnectionPoint[] mainPoints; // Точки для комплектующих (материнка, БП и т.д.)
    
    public bool IsFullyAssembled { get; private set; } = false;
    public PCStatus Status => currentStatus;

    private PCStatus currentStatus = PCStatus.NotWorking;
    private Dictionary<string, ConnectionPoint> pointStatuses = new();
    
    void Start()
    {
        // Автосбор точек или назначить вручную
        if (mainPoints.Length == 0) mainPoints = GetComponentsInChildren<ConnectionPoint>();
        
        foreach (var point in mainPoints) 
        {
            point.onStatusChanged += UpdateOverallStatus;
            pointStatuses[point.pointID] = point;
        }
    }
    
    private void UpdateOverallStatus(string pointID, ConnectionPoint point)
    {
        pointStatuses[pointID] = point;
        // Получаем все типы, которые должны быть (исключая NotSelected)
        var requiredTypes = System.Enum.GetValues(typeof(ComponentType))
            .Cast<ComponentType>()
            .Where(t => t != ComponentType.NotSelected);
        
        // Группируем точки по типу и проверяем наличие хотя бы одной FullySecured для каждого типа
        var groupedPoints = pointStatuses.Values
            .Where(p => p.Status == ComponentStatus.FullySecured)
            .GroupBy(p => p.connectionType);
        
        bool allTypesCovered = requiredTypes.All(reqType => groupedPoints.Any(group => group.Key == reqType));
        IsFullyAssembled = allTypesCovered;
        currentStatus = IsFullyAssembled ? PCStatus.Working : PCStatus.NotWorking;
        
        // Визуал/звук/feedback
        Debug.Log($"{transform.name} status: {currentStatus}");
    }
    
    // Для отладки
    void OnDestroy()
    {
        foreach (var point in mainPoints) point.onStatusChanged -= UpdateOverallStatus;
    }
}
