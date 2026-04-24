using System.Collections;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer)), RequireComponent(typeof(XRGrabInteractable))]
public class ComponentRaycast : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds5 = new(5f);
    public LayerMask interactableLayer;
    private LineRenderer lineRenderer;

    private Color rayColor = new(1f, 1f, 1f, 0.5f);
    private Color rayColorHit = Color.blue;

    public GameObject descriptionPanel;
    public TextMeshProUGUI text;
    public TextMeshProUGUI title;
    public HoldButton deleteButton;
    public Button questionButton;
    public Button showConnectionButton;
    public GameObject hightlightPointPrefab;
    private List<GameObject> hightlightPoints = new();
    public TextMeshProUGUI helpText;
    public ChangeUI changeUI;

    private GameObject deviceObject;
    private Coroutine showCoroutine;

    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;

    public int curveResolution = 30;
    public float detectionRadius = 0.05f;
    public float raycastDistance = 10f;

    private Material rayMaterial;
    private IXRSelectInteractor interactor;

    private string defaultText = "Наведитесь на объект и нажимте триггер.";
    private string defaultTitle = "Информация о комполектующих";

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = curveResolution;
        lineRenderer.startColor = rayColor;
        lineRenderer.endColor = rayColor;
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.enabled = false;

        descriptionPanel.SetActive(false);

        if (TryGetComponent<XRGrabInteractable>(out grabInteractable))
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }

        rayMaterial = lineRenderer.material;

        text.SetText(defaultText);
        title.SetText(defaultTitle);

        DisableDeleteButton();
    }

    public void EnableDeleteButton()
    {
        deleteButton.gameObject.SetActive(true);
        deleteButton.SetIsDisabled(true);
        text.SetText(defaultText);
        title.SetText(defaultTitle);
    }

    public void DisableDeleteButton()
    {
        deleteButton.gameObject.SetActive(false);
        deleteButton.SetIsDisabled(true);
        text.SetText(defaultText);
        title.SetText(defaultTitle);
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
        if (interactor != null)
        {
            if (interactor.transform.TryGetComponent<HandInfo>(out var info))
            {
                info.activateAction.performed += FireRay;
            }
        }
        foreach (var obj in hightlightPoints)
        {
            Destroy(obj);
        }
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (args.interactorObject.transform.gameObject.name == "Direct Interactor")
        {
            isGrabbed = true;
            descriptionPanel.SetActive(true);
            lineRenderer.enabled = true;
            interactor = args.interactorObject;
            if (interactor.transform.TryGetComponent<HandInfo>(out var info))
            {
                info.activateAction.performed += FireRay;
            }
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        descriptionPanel.SetActive(false);
        lineRenderer.enabled = false;
        if (interactor != null)
        {
            if (interactor.transform.TryGetComponent<HandInfo>(out var info))
            {
                info.activateAction.performed -= FireRay;
            }
            interactor = null;
        }
    }

    private RaycastHit? GetClosestHit()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        RaycastHit[] hits = Physics.SphereCastAll(origin, detectionRadius, direction, raycastDistance, interactableLayer, QueryTriggerInteraction.Ignore);
        RaycastHit? closestHit = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.collider.TryGetComponent<AttachObject>(out var attachObject))
            {
                attachObject = hit.collider.GetComponentInParent<AttachObject>();
            }
            if (attachObject != null && hit.distance < closestDist)
            {
                closestHit = hit;
                closestDist = hit.distance;
            }
        }

        return closestHit;
    }

    private string arrayToString(object arr)
    {
        return string.Join(", ", (arr as IEnumerable).Cast<object>().Select(x => x?.ToString() ?? "null"));
    }

    private void SetDeviceObject(GameObject device)
    {
        deviceObject = device;
        questionButton.interactable = true;
        showConnectionButton.interactable = true;
        if (deleteButton.enabled && deviceObject.TryGetComponent<AttachObjectDevice>(out var deviceInfo))
        {
            if (!deviceInfo.objIsAttached && !deviceInfo.IsAnyDeviceIsAttached)
            {
                deleteButton.SetIsDisabled(false);
            }
        }
        //if (deleteButton.enabled)
        //    deleteButton.SetIsDisabled(false);
    }

    private void ClearDeviceObject()
    {
        deviceObject = null;
        text.SetText(defaultText);
        title.SetText(defaultTitle);
        changeUI.ActivatePanel(0);
        questionButton.interactable = false;
        showConnectionButton.interactable = false;
        if (deleteButton.enabled)
            deleteButton.SetIsDisabled(true);
    }

    public void DeleteObject()
    {
        if (deviceObject == null)
        {
            ClearDeviceObject();
            return;
        }

        Destroy(deviceObject);
        ClearDeviceObject();
    }

    public void ShowConnections()
    {
        if (showCoroutine != null) return;
        showCoroutine = StartCoroutine(ShowConCoroutine());
    }

    private IEnumerator ShowConCoroutine()
    {
        float scanRadius = 5f;
        int layerMask = LayerMask.GetMask("PhysicObject");
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position,
            scanRadius,
            layerMask,
            QueryTriggerInteraction.Collide
        );

        List<GameObject> targetObjs = new();
        foreach (var col in hitColliders)
        {
            if (col.isTrigger && col.CompareTag(deviceObject.tag))
            {
                targetObjs.Add(col.gameObject);
            }
        }

        int instantiateCount = targetObjs.Count - hightlightPoints.Count;
        for (int i = 0; i < instantiateCount; i++)
        {
            GameObject point = Instantiate(hightlightPointPrefab);
            point.SetActive(false);
            hightlightPoints.Add(point);
        }

        for (int i = 0; i < targetObjs.Count; i++)
        {
            hightlightPoints[i].transform.SetParent(targetObjs[i].transform);
            hightlightPoints[i].transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            hightlightPoints[i].SetActive(true);
        }
        
        yield return _waitForSeconds5;

        for (int i = targetObjs.Count - 1; i >= 0; i--)
        {
            if (hightlightPoints[i] != null)
            {
                hightlightPoints[i].transform.SetParent(transform);
                hightlightPoints[i].transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                hightlightPoints[i].SetActive(false);
            } else {
                hightlightPoints.RemoveAt(i);
            }
        }
        showCoroutine = null;
    }

    private void FireRay(InputAction.CallbackContext context)
    {
        if (!isGrabbed) return;

        RaycastHit? closestValidHit = GetClosestHit();
        if (!closestValidHit.HasValue)
        {
            ClearDeviceObject();
            return;
        }
        
        if (!closestValidHit.Value.collider.TryGetComponent<AttachObject>(out var attachObject))
        {
            attachObject = closestValidHit.Value.collider.GetComponentInParent<AttachObject>();
        }

        SetDeviceObject(attachObject.gameObject);
        string newText = "";
        string newTitle = "";
        string newHelpText = "";

        if (attachObject is AttachObjectCable cable)
        {
            newTitle = "Кабель";
            newText = cable.tag switch
            {
                "CPU power" => "Тип кабеля: питание процессора",
                "GPU power" => "Тип кабеля: питание видеокарты",
                "Motherboard power" => "Тип кабеля: питание материнской платы",
                "SATA cable" => "Тип кабеля: SATA кабель для накопителя",
                _ => $"Тип кабеля: {cable.tag}",
            };
            newHelpText = "<color=#ADD8E6>Тип кабеля:</color> определяется назначением кабеля, его разъёмом. Используется для определения места подключения.";
            
            text.SetText(newText);
            title.SetText(newTitle);
            helpText.SetText(newHelpText);
            return;
        }

        if (attachObject is AttachObjectDevice device)
        {
            DeviceInfo deviceInfo = device.deviceInfo;

            switch (deviceInfo)
            {
                case CoolerInfo:
                    var coolerInfo = deviceInfo as CoolerInfo;
                    newText = "Тип комплектующего: Кулер\n" +
                    $"Название: {deviceInfo.Name}\n" +
                    $"Поддержка сокетов: {arrayToString(coolerInfo.SupportSockets)}\n" +
                    $"Лимит TDP: {coolerInfo.TDPLimit} Вт";
                    newTitle = $"{deviceInfo.Name}";
                    newHelpText = "<color=#ADD8E6>Поддержка сокетов:</color> указывает сокеты материнской платы, на которые возможна установка.\n" +
                    "<color=#ADD8E6>Лимит TDP:</color> максимальная рассеиваемая мощность тепла кулером от процессора. При превышении лимита происходит перегрев процессора.";
                    break;
                case CPUInfo:
                    var cpuInfo = deviceInfo as CPUInfo;
                    newText = "Тип комплектующего: Процессор\n" +
                    $"Название: {deviceInfo.Name}\n" +
                    $"Производитель: {cpuInfo.CPUManufacturer}\n" +
                    $"Модель: {cpuInfo.Model}\n" +
                    $"Тип сокета: {cpuInfo.SocketType}\n" +
                    $"Производительность: {cpuInfo.Performance}\n" +
                    $"TDP: {cpuInfo.TDP} Вт";
                    newTitle = $"{deviceInfo.Name}";
                    newHelpText = "<color=#ADD8E6>Производитель:</color> создатель данной модели. Используется для разделения моделей на группы.\n\n" +
                    "<color=#ADD8E6>Модель:</color> название модели. Одна модель может иметь несколько версий, созданные разными компаниями.\n\n" +
                    "<color=#ADD8E6>Тип сокета:</color> указывает сокет материнской платы, на который возможна установка.\n\n" +
                    "<color=#ADD8E6>Производительность:</color> синтетическая оценка производительности устройства.\n\n" +
                    "<color=#ADD8E6>TDP:</color> Расчётная тепловая мощность при работе. Данные используются для выбора подходящего кулера и вычисления потребления сборки.";
                    break;
                case GPUInfo:
                    var gpuInfo = deviceInfo as GPUInfo;
                    newText = "Тип комплектующего: Видеокарта\n" +
                    $"Название: {deviceInfo.Name}\n" +
                    $"Производитель: {gpuInfo.GPUManufacturer}\n" +
                    $"Модель: {gpuInfo.Model}\n" +
                    $"Объем памяти: {gpuInfo.MemoryAmountGB} ГБ\n" +
                    $"Поддержка PCI-E: {gpuInfo.PCIESupport}\n" +
                    $"Производительность: {gpuInfo.Performance}\n" +
                    $"TDP: {gpuInfo.TDP} Вт";
                    newTitle = $"{deviceInfo.Name}";
                    newHelpText = "<color=#ADD8E6>Производитель:</color> создатель данной модели. Используется для разделения моделей на группы.\n\n" +
                    "<color=#ADD8E6>Модель:</color> название модели. Одна модель может иметь несколько версий, созданные разными компаниями.\n\n" +
                    "<color=#ADD8E6>Объём памяти:</color> количество видеопамяти. Хранит данные, необходимые для вычислений, влияет на производительность.\n\n" +
                    "<color=#ADD8E6>Поддержка PCI-E:</color> максимальная поддерживаемая версия PCI-E. Влияет на скорость передачи данных к материнской плате и не влияет на совместимость.\n\n" +
                    "<color=#ADD8E6>Производительность:</color> синтетическая оценка производительности устройства.\n\n" +
                    "<color=#ADD8E6>TDP:</color> Расчётная тепловая мощность при работе. Данные используются для вычисления потребления сборки.";
                    break;
                case MotherboardInfo:
                    var motherboardInfo = deviceInfo as MotherboardInfo;
                    newText = "Тип комплектующего: Материнская плата\n" +
                    $"Название: {deviceInfo.Name}\n" +
                    $"Поддержка CPU: {motherboardInfo.CPUManufacturer}\n" +
                    $"Тип сокета: {motherboardInfo.SocketType}\n" +
                    $"Поддержка PCI-E: {motherboardInfo.PCIESupport}\n" +
                    $"Тип памяти: {motherboardInfo.DDRType}";
                    newTitle = $"{deviceInfo.Name}";
                    newHelpText = "<color=#ADD8E6>Поддержка CPU:</color> указывает для какого производителя процессоров нацелена материнская плата. Используется для разделения на группы.\n\n" +
                    "<color=#ADD8E6>Тип сокета:</color> указывает сокет материнской платы. Используется для проверки совместимости процессора.\n\n" +
                    "<color=#ADD8E6>Поддержка PCI-E:</color> максимальная поддерживаемая версия PCI-E. Влияет на скорость передачи данных к видеокарте и не влияет на совместимость.\n\n" +
                    "<color=#ADD8E6>Тип памяти:</color> указывает поддерживаемый тип оперативной памяти. Другие типы несовместимы.";
                    break;
                case RAMInfo:
                    var ramInfo = deviceInfo as RAMInfo;
                    newText = "Тип комплектующего: Оперативная память\n" +
                    $"Название: {deviceInfo.Name}\n" +
                    $"Тип памяти: {ramInfo.DDRType}\n" +
                    $"Объем памяти: {ramInfo.MemoryAmountGB} ГБ\n" +
                    $"Частота памяти: {ramInfo.FrequencyMhz} Мгц";
                    newTitle = $"{deviceInfo.Name}";
                    newHelpText = "<color=#ADD8E6>Тип памяти:</color> тип оперативной памяти. Используется для проверки совместимости с материнской платой.\n\n" +
                    "<color=#ADD8E6>Объём памяти:</color> количество оперативной памяти. Хранит данные, необходимые для вычислений, влияет на производительность.\n\n" +
                    "<color=#ADD8E6>Частота памяти:</color> частота выполнений операций оперативной памятью. Влияет на производительность.";
                    break;
                case PowerSupplyInfo:
                    var powerSupplyInfo = deviceInfo as PowerSupplyInfo;
                    newText = "Тип комплектующего: Блок питания\n" +
                    $"Название: {deviceInfo.Name}\n" +
                    $"Максимальная мощность: {powerSupplyInfo.PowerSupplyMaxPower} Вт";
                    newTitle = $"{deviceInfo.Name}";
                    newHelpText = "<color=#ADD8E6>Максимальная мощность:</color> максимальная выдаваемая мощность блоком питания. Готовая сборка должна потреблять меньше, чем указанное значение. Вычисление примерного потребления сборки:\n" +
                    "Потребление = TDP_процессора + TDP_видеокарты + 100 Вт.";
                    break;
                case StorageDeviceInfo:
                    var storageDeviceInfo = deviceInfo as StorageDeviceInfo;
                    newText = "Тип комплектующего: Накопитель данных\n" +
                    $"Название: {deviceInfo.Name}\n" +
                    $"Тип накопителя: {storageDeviceInfo.StorageDeviceType}\n" +
                    $"Объем памяти: {storageDeviceInfo.MemoryAmountGB} ГБ";
                    newTitle = $"{deviceInfo.Name}";
                    newHelpText = "<color=#ADD8E6>Тип накопителя:</color> указывает на тип используемой памяти и разъёма подключения накопителем. Влияет на скорость загрузки данных в оперативную память.\n\n" +
                    "<color=#ADD8E6>Объём памяти:</color> количество памяти в накопителе. Хранит все данные компьютера.";
                    break;
            }

            text.SetText(newText);
            title.SetText(newTitle);
            helpText.SetText(newHelpText);
            return;
        }
    }

    
    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (isGrabbed)
        {
            var closestHit = GetClosestHit();

            Vector3 startPoint = origin;
            Vector3 endPoint = origin + direction * raycastDistance;
            Vector3[] points = new Vector3[curveResolution];

            if (closestHit.HasValue)
            {
                rayMaterial.color = rayColorHit;
                endPoint = closestHit.Value.point;

                for (int i = 0; i < curveResolution; i++)
                {
                    float t = i / (float)(curveResolution - 1);
                    points[i] = GetCurvePoint(t, startPoint, endPoint);
                }
            }
            else
            {
                rayMaterial.color = rayColor;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, startPoint);
                lineRenderer.SetPosition(1, endPoint);
                return;
            }

            lineRenderer.positionCount = curveResolution;
            lineRenderer.SetPositions(points);
        }
    }

    private Vector3 GetCurvePoint(float t, Vector3 p1, Vector3 p2)
    {
        float easedT = 1f - Mathf.Pow(1f - t, 3f);
        return Vector3.Lerp(p1, p2, easedT);
    }
}
