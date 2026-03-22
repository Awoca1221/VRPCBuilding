using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildStatusView : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new(1f);

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
    public TextMeshProUGUI performanceText;
    public Transform statusContentTransform;
    public GameObject statusElementPrefab;
    public Transform buildContentTransform;
    public GameObject buildElementPrefab;
    public GameObject savePanel;
    public Button saveButton;
    public Button spawnButton;
    public TextMeshProUGUI saveSuccess;
    public bool enableSaveOption = false;

    private TooltipHandler saveTooltip;
    private readonly List<GameObject> errorElems = new();
    private readonly List<GameObject> buildElems = new();
    private PlayerBuildsList buildsData;
    private int lastSelectedBuildIndex = -1;
    private bool buildIsLoading;
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateStatus();
        UpdatePlayerBuilds();
        UpdateSpawnButton();
        PCBuild.OnOverallStatusUpdated += UpdateStatus;
        PCBuild.OnOverallStatusUpdated += UpdateSpawnButton;
        SaveService.onSave += OnBuildsDataUpdate;
        saveTooltip = saveButton.GetComponent<TooltipHandler>();
        saveButton.interactable = false;
        spawnButton.interactable = false;
        if (!enableSaveOption)
        {
            savePanel.SetActive(false);
        } else {
            savePanel.SetActive(true);
        }
    }

    void OnDestroy()
    {
        PCBuild.OnOverallStatusUpdated -= UpdateStatus;
        PCBuild.OnOverallStatusUpdated -= UpdateSpawnButton;
        SaveService.onSave -= OnBuildsDataUpdate;
        foreach (var elem in errorElems)
        {
            elem.SetActive(false);
            Destroy(elem);
        }
        errorElems.Clear();
        foreach (var elem in buildElems)
        {
            elem.SetActive(false);
            Destroy(elem);
        }
        buildElems.Clear();
    }

    private void UpdateStatus()
    {
        // Удаление всех элементов в списке
        foreach (var elem in errorElems)
        {
            elem.SetActive(false);
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
                AddCustomElement("Критическое потребление питания сборкой. Требуется блок питания с большей мощностью");
                break;
            case PCBuildManager.Status.Working:
                totalStatusText.text = "Статус компьютера: работает";
                break;
        }

        // Определение производительности сборки
        if (PCBuild.PCStatus == PCBuildManager.Status.Working)
        {
            PCBuildManager.CPUPerformance cpuPerformance = PCBuild.GetCPUPerformance();
            if (cpuPerformance.isOverheated)
            {
                AddCustomElement("Процессор перегревается, производительность снижена");
            }
            if (cpuPerformance.isWithoutPaste)
            {
                AddCustomElement("Процессор без термопасты, производительность значительно снижена");
            }
            performanceText.text =
            "Оценка процессора: " + cpuPerformance.performance + " балла(ов)\n" +
            "Оценка видеокарты: " + PCBuild.GetGPUPerformance() + " балла(ов)\n" +
            "Общая оценка производительности: " + PCBuild.GetOverallPerformance() + " балла(ов)";
            saveTooltip.isEnabled = false;
            saveButton.interactable = true;
        }
        else
        {
            performanceText.text = "Невозможно оценить производительность не собранной сборки";
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
            var attachObject = device.GetComponent<AttachObjectDevice>();
            AttachObjectDevice.Status deviceStatus = attachObject.DeviceStatus;
            ComponentType deviceType = attachObject.deviceInfo.ComponentType;
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
        GameObject createdElement = Instantiate(statusElementPrefab, statusContentTransform);
        createdElement.GetComponent<BuildStatusElement>().SetText(errorMessage);
        errorElems.Add(createdElement);
    }

    private void AddCustomElement(string errorMessage)
    {
        if (errorMessage == null) return;
        GameObject createdElement = Instantiate(statusElementPrefab, statusContentTransform);
        createdElement.GetComponent<BuildStatusElement>().SetText(errorMessage);
        errorElems.Add(createdElement);
    }

    private void OnBuildsDataUpdate(string key)
    {
        if (key == "player_builds")
        {
            UpdatePlayerBuilds();
        }
    }

    public void UpdatePlayerBuilds()
    {
        // Удаление всех элементов в списке
        lastSelectedBuildIndex = -1;
        foreach (var elem in buildElems)
        {
            elem.SetActive(false);
            Destroy(elem);
        }
        buildElems.Clear();

        buildsData = SaveService.Load<PlayerBuildsList>("player_builds");

        // заполнение префабов информацией
        int count = buildsData.builds.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject createdElement = Instantiate(buildElementPrefab, buildContentTransform);
            int index = i;
            createdElement.GetComponent<BuildElement>().SetData(buildsData.builds[i], () => {
                HandleSelectBuild(index);
            }, () => {
                HandleDeleteBuild(index);
            });
            buildElems.Add(createdElement);
        }
    }

    private void HandleSelectBuild(int index)
    {
        if (lastSelectedBuildIndex != -1)
        {
            buildElems[lastSelectedBuildIndex].GetComponent<BuildElement>().SetSelect(false);
        }
        buildElems[index].GetComponent<BuildElement>().SetSelect(true);
        lastSelectedBuildIndex = index;
        UpdateSpawnButton();
    }

    private void HandleDeleteBuild(int index)
    {
        buildsData.builds.RemoveAt(index);
        SaveService.Save("player_builds", buildsData);
    }

    public void SaveBuild()
    {
        PCBuild.SaveBuild();
        StartCoroutine(ShowSaveSuccess());
        UpdatePlayerBuilds();
    }

    public void SpawnBuild()
    {
        if (lastSelectedBuildIndex != -1)
        {
            /*
            buildIsLoading = true;
            PCBuild.SpawnBuild(buildsData.builds[lastSelectedBuildIndex], OnBuildSpawned);
            spawnButton.interactable = false;
            */
            if (PCBuild.IsSpawnAreaClear())
            {
                buildIsLoading = true;
                PCBuild.SpawnBuild(buildsData.builds[lastSelectedBuildIndex], OnBuildSpawned);
                spawnButton.interactable = false;
            }
            else
            {
                spawnButton.GetComponent<TooltipHandler>().ShowTooltip(2f);
            }
        }
    }

    private void OnBuildSpawned()
    {
        buildIsLoading = false;
    }

    public void UpdateSpawnButton()
    {
        if (PCBuild.CountConnectedDevices == 0 && !buildIsLoading)
        {
            spawnButton.interactable = true;
        } else {
            spawnButton.interactable = false;
        }
    }

    private IEnumerator ShowSaveSuccess()
    {
        saveButton.interactable = false;
        float fadeDuration = 0.3f;
        while (saveSuccess.alpha < 1f)
        {
            saveSuccess.alpha += Time.deltaTime / fadeDuration;
            yield return null;
        }

        yield return _waitForSeconds1;

        while (saveSuccess.alpha > 0f)
        {
            saveSuccess.alpha -= Time.deltaTime / fadeDuration;
            yield return null;
        }
        saveButton.interactable = true;
    }
}
