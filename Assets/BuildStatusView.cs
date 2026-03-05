using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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
    public Transform contentTransform;
    public GameObject elementPrefab;

    private readonly List<GameObject> elements = new();
    private readonly float offset = 20f;
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateInfo();
        PCBuild.OnOverallStatusUpdated += UpdateInfo;
    }

    private void UpdateInfo()
    {
        // Удаление всех элементов в списке
        foreach (var elem in elements)
        {
            Destroy(elem);
        }
        elements.Clear();

        // Определение статуса компьютера
        if (PCBuild.PCStatus == PCBuildManager.Status.NotWorking)
        {
            totalStatusText.text = "Статус компьютера: не работает";
        }
        else
        {
            totalStatusText.text = "Статус компьютера: работает";
            return;
        }

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
        elements.Add(createdElement);
    }
}
