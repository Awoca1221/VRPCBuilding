using System.Collections;
using System.Linq;
using TMPro;
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

    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;

    public InputActionReference fireReference = null;

    public int curveResolution = 30;
    public float detectionRadius = 0.05f;
    public float raycastDistance = 10f;

    private Material rayMaterial;

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

        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }

        rayMaterial = lineRenderer.material;
        fireReference.action.started += FireRay;

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
        fireReference.action.started -= FireRay;
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    private RaycastHit? GetClosestHit()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        RaycastHit[] hits = Physics.SphereCastAll(origin, detectionRadius, direction, raycastDistance, interactableLayer);
        RaycastHit? closestHit = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            ItemCommon itemCommon = hit.collider.GetComponent<ItemCommon>();
            if (itemCommon != null && hit.distance < closestDist)
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

    private void FireRay(InputAction.CallbackContext context)
    {
        if (isGrabbed)
        {
            RaycastHit? closestValidHit = GetClosestHit();

            if (closestValidHit.HasValue)
            {
                BaseInfo baseInfo = closestValidHit.Value.collider.GetComponent<ItemCommon>().GetInfo();
                string newText = "";
                string newTitle = "";
                switch (baseInfo)
                {
                    case CoolerInfo:
                        var coolerInfo = baseInfo as CoolerInfo;
                        newText = @$"Тип комплектующего: Кулер
Поддержка сокетов: {arrayToString(coolerInfo.SupportSockets)}
Лимит TDP: {coolerInfo.TDPLimit}";
                        newTitle = $"";
                        break;
                    case CPUInfo:
                        var cpuInfo = baseInfo as CPUInfo;
                        newText = @$"Тип комплектующего: Процессор
Производитель: {cpuInfo.CPUManufacturer}
Модель: {cpuInfo.Model}
Тип сокета: {cpuInfo.SocketType}
Производительность: {cpuInfo.Performance}";
                        newTitle = $"";
                        break;
                    case GPUInfo:
                        var gpuInfo = baseInfo as GPUInfo;
                        newText = @$"Тип комплектующего: Видеокарта
Производитель: {gpuInfo.GPUManufacturer}
Модель: {gpuInfo.Model}
Объем памяти: {gpuInfo.MemoryAmountGB} ГБ
Поддержка PCI-E: {gpuInfo.PCIESupport}
Производительность: {gpuInfo.Performance}";
                        newTitle = $"";
                        break;
                    case MotherboardInfo:
                        var motherboardInfo = baseInfo as MotherboardInfo;
                        newText = @$"Тип комплектующего: Материнская плата
Поддержка CPU: {motherboardInfo.CPUManufacturer}
Тип сокета: {motherboardInfo.SocketType}
Поддержка PCI-E: {motherboardInfo.PCIESupport}
Тип памяти: {motherboardInfo.DDRType}";
                        newTitle = $"";
                        break;
                    case RAMInfo:
                        var ramInfo = baseInfo as RAMInfo;
                        newText = @$"Тип комплектующего: Оперативная память
Тип памяти: {ramInfo.DDRType}
Объем памяти: {ramInfo.MemoryAmountGB} ГБ";
                        newTitle = $"";
                        break;
                    case PowerSupplyInfo:
                        var powerSupplyInfo = baseInfo as PowerSupplyInfo;
                        newText = @$"Тип комплектующего: Блок питания
Максимальная мощность: {powerSupplyInfo.PowerSupplyMaxPower} Вт";
                        newTitle = $"";
                        break;
                    case StorageDeviceInfo:
                        var storageDeviceInfo = baseInfo as StorageDeviceInfo;
                        newText = @$"Тип комплектующего: Накопитель данных
Тип накопителя: {storageDeviceInfo.StorageDeviceType}
Объем памяти: {storageDeviceInfo.MemoryAmountGB} ГБ";
                        newTitle = $"";
                        break;
                }
                text.SetText(newText);
                title.SetText(newTitle);
            }
            else
            {
                text.SetText(defaultText);
                title.SetText(defaultTitle);
            }
        }
    }

    
    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (isGrabbed)
        {
            descriptionPanel.SetActive(true);
            lineRenderer.enabled = true;

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
        else
        {
            descriptionPanel.SetActive(false);
            lineRenderer.enabled = false;
        }
    }

    private Vector3 GetCurvePoint(float t, Vector3 p1, Vector3 p2)
    {
        float easedT = 1f - Mathf.Pow(1f - t, 3f);
        return Vector3.Lerp(p1, p2, easedT);
    }
}
