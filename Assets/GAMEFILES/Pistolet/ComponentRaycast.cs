using System.Collections;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(LineRenderer)), RequireComponent(typeof(XRGrabInteractable))]
public class ComponentRaycast : MonoBehaviour
{
    public LayerMask interactableLayer;
    private LineRenderer lineRenderer;

    private Color rayColor = new(1f, 1f, 1f, 0.5f);
    private Color rayColorHit = Color.blue;

    public GameObject descriptionPanel;
    public TextMeshProUGUI text;
    public TextMeshProUGUI title;
    public HoldButton deleteButton;

    private GameObject deviceObject;

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
            if (!hit.collider.TryGetComponent<AttachObjectDevice>(out var attachObjectDevice))
            {
                attachObjectDevice = hit.collider.GetComponentInParent<AttachObjectDevice>();
            }
            if (attachObjectDevice != null && hit.distance < closestDist)
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
        if (deleteButton.enabled)
        {
            AttachObjectDevice deviceInfo = deviceObject.GetComponent<AttachObjectDevice>();
            if (!deviceInfo.objIsAttached && !deviceInfo.IsAnyDeviceIsAttached)
            {
                deleteButton.SetIsDisabled(false);
            }
        }
        //if (deleteButton.enabled)
        //    deleteButton.SetIsDisabled(false);
    }

    public void DeleteObject()
    {
        if (deviceObject == null) return;

        deleteButton.SetIsDisabled(true);
        Destroy(deviceObject);
        text.SetText(defaultText);
        title.SetText(defaultTitle);
    }

    private void FireRay(InputAction.CallbackContext context)
    {
        if (isGrabbed)
        {
            RaycastHit? closestValidHit = GetClosestHit();

            if (closestValidHit.HasValue)
            {
                if (!closestValidHit.Value.collider.TryGetComponent<AttachObjectDevice>(out var attachObjectDevice))
                {
                    attachObjectDevice = closestValidHit.Value.collider.GetComponentInParent<AttachObjectDevice>();
                }
                SetDeviceObject(attachObjectDevice.gameObject);
                DeviceInfo deviceInfo = attachObjectDevice.deviceInfo;
                string newText = "";
                string newTitle = "";
                switch (deviceInfo)
                {
                    case CoolerInfo2:
                        var coolerInfo = deviceInfo as CoolerInfo2;
                        newText = @$"Тип комплектующего: Кулер
Название: {deviceInfo.Name}
Поддержка сокетов: {arrayToString(coolerInfo.SupportSockets)}
Лимит TDP: {coolerInfo.TDPLimit}";
                        newTitle = $"{deviceInfo.Name}";
                        break;
                    case CPUInfo2:
                        var cpuInfo = deviceInfo as CPUInfo2;
                        newText = @$"Тип комплектующего: Процессор
Название: {deviceInfo.Name}
Производитель: {cpuInfo.CPUManufacturer}
Модель: {cpuInfo.Model}
Тип сокета: {cpuInfo.SocketType}
Производительность: {cpuInfo.Performance}
TDP: {cpuInfo.TDP}";
                        newTitle = $"{deviceInfo.Name}";
                        break;
                    case GPUInfo2:
                        var gpuInfo = deviceInfo as GPUInfo2;
                        newText = @$"Тип комплектующего: Видеокарта
Название: {deviceInfo.Name}
Производитель: {gpuInfo.GPUManufacturer}
Модель: {gpuInfo.Model}
Объем памяти: {gpuInfo.MemoryAmountGB} ГБ
Поддержка PCI-E: {gpuInfo.PCIESupport}
Производительность: {gpuInfo.Performance}
TDP: {gpuInfo.TDP}";
                        newTitle = $"{deviceInfo.Name}";
                        break;
                    case MotherboardInfo2:
                        var motherboardInfo = deviceInfo as MotherboardInfo2;
                        newText = @$"Тип комплектующего: Материнская плата
Название: {deviceInfo.Name}
Поддержка CPU: {motherboardInfo.CPUManufacturer}
Тип сокета: {motherboardInfo.SocketType}
Поддержка PCI-E: {motherboardInfo.PCIESupport}
Тип памяти: {motherboardInfo.DDRType}";
                        newTitle = $"{deviceInfo.Name}";
                        break;
                    case RAMInfo2:
                        var ramInfo = deviceInfo as RAMInfo2;
                        newText = @$"Тип комплектующего: Оперативная память
Название: {deviceInfo.Name}
Тип памяти: {ramInfo.DDRType}
Объем памяти: {ramInfo.MemoryAmountGB} ГБ
Частота памяти: {ramInfo.FrequencyMhz} Мгц";
                        newTitle = $"{deviceInfo.Name}";
                        break;
                    case PowerSupplyInfo2:
                        var powerSupplyInfo = deviceInfo as PowerSupplyInfo2;
                        newText = @$"Тип комплектующего: Блок питания
Название: {deviceInfo.Name}
Максимальная мощность: {powerSupplyInfo.PowerSupplyMaxPower} Вт";
                        newTitle = $"{deviceInfo.Name}";
                        break;
                    case StorageDeviceInfo2:
                        var storageDeviceInfo = deviceInfo as StorageDeviceInfo2;
                        newText = @$"Тип комплектующего: Накопитель данных
Название: {deviceInfo.Name}
Тип накопителя: {storageDeviceInfo.StorageDeviceType}
Объем памяти: {storageDeviceInfo.MemoryAmountGB} ГБ";
                        newTitle = $"{deviceInfo.Name}";
                        break;
                }
                text.SetText(newText);
                title.SetText(newTitle);
            }
            else
            {
                text.SetText(defaultText);
                title.SetText(defaultTitle);
                if (deleteButton.enabled)
                    deleteButton.SetIsDisabled(true);
            }
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
