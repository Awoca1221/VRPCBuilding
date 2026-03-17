using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildStatusView : MonoBehaviour
{
    public enum ErrorType
    {
        NotInserted,
        InsertedLoose
    }

    private static readonly Dictionary<ErrorType, string> _errorMessages = new()
    {
        { ErrorType.NotInserted, "Отсутствует часть: " },
        { ErrorType.InsertedLoose, "Не завершена установка части: " }
    };
    private static readonly Dictionary<ComponentType, string> _componentMessages = new()
    {
        { ComponentType.Cooler, "кулер" },
        { ComponentType.CPU, "процессор" },
        { ComponentType.GPU, "видеокарта" },
        { ComponentType.RAM, "оперативная память" },
        { ComponentType.Motherboard, "материнская плата" },
        { ComponentType.PowerSupply, "блок питания" },
        { ComponentType.StorageDevice, "накопитель" }
    };

    public PCBuildManager PCBuild;
    [Space]
    public TextMeshProUGUI totalStatusText;
    public TextMeshProUGUI CPUPerfText;
    public TextMeshProUGUI GPUPerfText;
    public TextMeshProUGUI PCPerfText;
    public Transform contentTransform;
    public GameObject elementPrefab;

    private readonly List<GameObject> errorElems = new();
    private readonly List<GameObject> buildElems = new();
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateStatus();
        UpdatePlayerBuilds();
        PCBuild.OnOverallStatusUpdated += UpdateStatus;
    }

    private void UpdateStatus()
    {
        // Удаление всех элементов в списке
        foreach (var elem in errorElems)
        {
            Destroy(elem);
        }
        errorElems.Clear();

        // Определение статуса компьютера
        switch (PCBuild.PCStatus)
        {
            case PCBuildManager.Status.NotWorking:
                totalStatusText.text = "Статус компьютера: не работает";
                break;
            case PCBuildManager.Status.Unstable:
                totalStatusText.text = "Статус компьютера: нестабилен";
                AddNotEnoughPowerElement();
                break;
            case PCBuildManager.Status.Working:
                totalStatusText.text = "Статус компьютера: работает";
                break;
        }

        // Определение производительности сборки
        if (PCBuild.PCStatus == PCBuildManager.Status.Working)
        {
            CPUPerfText.text = "Оценка процессора: " + PCBuild.GetCPUPerformance() + " баллов";
            GPUPerfText.text = "Оценка видеокарты: " + PCBuild.GetGPUPerformance() + " баллов";
            PCPerfText.text = "Общая оценка производительности: " + PCBuild.GetOverallPerformance() + " баллов";
        }
        else
        {
            CPUPerfText.text = "Невозможно оценить производительность не собранной сборки";
            GPUPerfText.text = "";
            PCPerfText.text = "";
        }

        if (PCBuild.PCStatus == PCBuildManager.Status.Working || PCBuild.PCStatus == PCBuildManager.Status.Unstable) return;
        
        // Определение проблем в сборке
        var requiredTypes = System.Enum.GetValues(typeof(ComponentType))
            .Cast<ComponentType>()
            .Where(t => t != ComponentType.NotSelected);
        HashSet<ComponentType> typesCovered = new();

        IReadOnlyCollection<GameObject> devices = PCBuild.ConnectedDevices;
        foreach (var device in devices)
        {
            AttachObjectDevice.Status deviceStatus = device.GetComponent<AttachObjectDevice>().DeviceStatus;
            ComponentType deviceType = device.GetComponent<ItemCommon>().ComponentType;
            if (deviceStatus == AttachObjectDevice.Status.InsertedLoose)
            {
                AddElement(ErrorType.InsertedLoose, deviceType);
            }
            typesCovered.Add(deviceType);
        }

        List<ComponentType> typesNotCovered = requiredTypes.Except(typesCovered).ToList();
        foreach (var type in typesNotCovered)
        {
            AddElement(ErrorType.NotInserted, type);
        }
    }

    private void AddElement(ErrorType error, ComponentType deviceType)
    {
        string errorMessage = _errorMessages[error] + _componentMessages[deviceType];
        GameObject createdElement = Instantiate(elementPrefab, contentTransform);
        BuildStatusElement elemStatus = createdElement.GetComponent<BuildStatusElement>();
        elemStatus.SetText(errorMessage);
        errorElems.Add(createdElement);
    }

    private void AddNotEnoughPowerElement()
    {
        string errorMessage = "Критическое потребление питания сборкой. Требуется блок питания с большей мощностью";
        GameObject createdElement = Instantiate(elementPrefab, contentTransform);
        BuildStatusElement elemStatus = createdElement.GetComponent<BuildStatusElement>();
        elemStatus.SetText(errorMessage);
        errorElems.Add(createdElement);
    }

    public void UpdatePlayerBuilds()
    {
        // Удаление всех элементов в списке
        foreach (var elem in buildElems)
        {
            Destroy(elem);
        }
        buildElems.Clear();

        var savedData = SaveService.Load<PlayerBuildsList>("player_builds");

        // заполнение префабов информацией
    }

    public void SaveBuild()
    {
        PCBuild.SaveBuild();
        UpdatePlayerBuilds();
    }
}
