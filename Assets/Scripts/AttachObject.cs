using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public enum ComponentStatus
{
    NotInserted,     // Не вставлен
    InsertedLoose,   // Вставлен, но не закреплён
    FullySecured     // Полностью подключён (все винты)
}

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable), typeof(ItemCommon))]
[RequireComponent(typeof(Outline))]
public class AttachObject : MonoBehaviour
{
    public GameObject attachPoint;
    private Material invis;
    public Material correct;
    public Material show;
    public Material wrong;

    public ComponentStatus CompStatus { get; private set; } = ComponentStatus.NotInserted;
    public PCBuildManager PCBuildRef { get; private set; } = null;

    private ConnectionPoint[] conPoints;
    private ScrewPoint[] screwPoints;
    private XRInteractionManager interactionManager;
    private IXRSelectInteractor interactor;
    private XRGrabInteractable interactable;
    private Collider checkCollider;
    private Vector3 saveScale;
    private bool ObjIsAttached => CompStatus != ComponentStatus.NotInserted;
    private ItemCommon objectInfo;
    [field: NonSerialized] public GameObject cpuConnected;
    private HashSet<GameObject> _connectedParts = new();
    private GameObject _highlightParent = null;
    private Material _currentMatForHightlight = null;
    private Collider _currentColliderForHighlight = null;

    [field: SerializeField] public CheckMultipleConnections MultipleConnections { get; set; } = null;
    [field: SerializeField] public UnityEvent OnConnectEvents { get; set; } = null;
    [field: SerializeField] public UnityEvent OnDisconnectEvents { get; set; } = null;

    //Debug.Log("...");
    void Start()
    {
        conPoints = GetComponentsInChildren<ConnectionPoint>();
        screwPoints = GetComponentsInChildren<ScrewPoint>();
        foreach (var screw in screwPoints) screw.onStatusChanged += OnScrewChangeStatus;
        GetComponent<Outline>().enabled = false;
        interactable = GetComponent<XRGrabInteractable>();
        objectInfo = GetComponentInParent<ItemCommon>();
        interactionManager = GameObject.Find("XR Interaction Manager").GetComponent<XRInteractionManager>();

        // Создать модель для выделения места подключения
        StartCoroutine(CreateHighlight());

        // Отслеживание нажатия кнопки для подключения и отключения объекта
        interactable.selectEntered.AddListener(OnGrabEnter);
        interactable.selectExited.AddListener(OnGrabExit);

        saveScale = transform.localScale;
        if (MultipleConnections)
        {
            OnConnectEvents.AddListener(MultipleConOnConnect);
            OnDisconnectEvents.AddListener(MultipleConOnDisconnect);
        }

        invis = Resources.Load<Material>("Materials/Invis");
        if (correct == null)
        {
            correct = Resources.Load<Material>("Materials/Correct");
        }
        if (show == null)
        {
            show = Resources.Load<Material>("Materials/Show");
        }
        if (wrong == null)
        {
            wrong = Resources.Load<Material>("Materials/Wrong");
        }
    }

    private void OnScrewChangeStatus()
    {
        if (screwPoints.Any(s => s.IsSecured))
        {
            interactable.enabled = false;
        }
        else
        {
            interactable.enabled = true;
        }
    }

    // Методы для отслеживания задач с множественным подключением (оперативная память)
    private void MultipleConOnConnect()
    {
        MultipleConnections.ConnectObject(gameObject);
    }

    private void MultipleConOnDisconnect()
    {
        MultipleConnections.DisconnectObject(gameObject);
    }
    
    // Методы для игнорирования столкновений между подключёнными объектами
    private void AddConnectedPart(GameObject part)
    {
        Collider[] partCols = part.GetComponentsInChildren<Collider>();
        foreach(GameObject conPart in _connectedParts)
        {
            Collider[] conPartCols = conPart.GetComponentsInChildren<Collider>();
            foreach(Collider partCol in partCols)
            {
                foreach(Collider conPartCol in conPartCols)
                {
                    Physics.IgnoreCollision(conPartCol, partCol, true);
                }
            }
        }
        _connectedParts.Add(part);
    }

    private void RemoveConnectedPart(GameObject part)
    {
        _connectedParts.Remove(part);
        Collider[] partCols = part.GetComponentsInChildren<Collider>();
        foreach(GameObject conPart in _connectedParts)
        {
            Collider[] conPartCols = conPart.GetComponentsInChildren<Collider>();
            foreach(Collider partCol in partCols)
            {
                foreach(Collider conPartCol in conPartCols)
                {
                    Physics.IgnoreCollision(conPartCol, partCol, false);
                }
            }
        }
    }

    // Методы для настройки отслеживания подключения/отключения объекта через триггер
    private void OnGrabEnter(SelectEnterEventArgs args)
    {
        interactor = args.interactorObject;
        if (interactor.transform.TryGetComponent<HandInfo>(out var info))
        {
            info.activateAction.performed += TryActivateAction;
        }
        
    }

    private void OnGrabExit(SelectExitEventArgs args)
    {
        interactor = args.interactorObject;
        if (interactor.transform.TryGetComponent<HandInfo>(out var info))
        {
            info.activateAction.performed -= TryActivateAction;
        }
    }

    // Активация кнопки подключения/отключения объекта
    private void TryActivateAction(InputAction.CallbackContext context)
    {
        if (ObjIsAttached)
            TryUnattach();
        else
            TryAttach();
    }

    // Подключение объекта
    private void TryAttach()
    {
        if (checkCollider != null && Quaternion.Angle(attachPoint.transform.rotation, checkCollider.gameObject.transform.rotation) <= 40)
        {
            // Сохранение прошлой иерархии и смена на новую
            Transform oldPlace = transform.parent;
            attachPoint.transform.SetParent(checkCollider.gameObject.transform, true);
            transform.SetParent(attachPoint.transform, true);

            // Сдвиг объекта на место подключения
            attachPoint.transform.SetLocalPositionAndRotation(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f));

            // Возврат к прошлой иерархии и восстановление размера объекта
            transform.SetParent(oldPlace, true);
            attachPoint.transform.SetParent(transform, true);
            transform.localScale = saveScale;

            // Скрепление объекта с разъёмом через FixedJoint
            transform.AddComponent<FixedJoint>();
            transform.GetComponent<FixedJoint>().connectedBody = checkCollider.GetComponentInParent<Rigidbody>();

            checkCollider.tag = "Unavailable";
            CompStatus = ComponentStatus.InsertedLoose;
            if (interactor != null)
                interactionManager.SelectExit(interactor, interactable);
            
            // Необходимо для отключения столкновений коллайдеров
            AddConnectedPart(checkCollider.transform.parent.GameObject());

            foreach (var screw in screwPoints) screw.SetAvailable();
            if (checkCollider.TryGetComponent<ConnectionPoint>(out var conPoint))
            {
                conPoint.OnConnect(gameObject);
                Transform conPointParent = conPoint.transform.parent;
                if (conPointParent.TryGetComponent<PCBuildManager>(out var pcBuildRef))
                {
                    SetPCBuildRef(pcBuildRef);
                }
                else if (conPointParent.TryGetComponent<AttachObject>(out var attachDevice))
                {
                    SetPCBuildRef(attachDevice.PCBuildRef);
                }
            }

            // Необходимо для блокировки процессора при подключении кулера
            AttachObject connectTo = checkCollider.GetComponentInParent<AttachObject>();
            if (connectTo == null)
            {
                OnConnectEvents.Invoke();
                return;
            }
            if (objectInfo.ComponentType == ComponentType.CPU)
            {
                connectTo.cpuConnected = gameObject;
            }
            if (objectInfo.ComponentType == ComponentType.Cooler && connectTo.cpuConnected)
            {
                connectTo.cpuConnected.GetComponent<XRGrabInteractable>().enabled = false;
            }
            OnConnectEvents.Invoke();
        }
    }

    // Отключение объекта
    private void TryUnattach()
    {
        if (ObjIsAttached)
        {
            checkCollider.tag = tag;
            Destroy(GetComponent<FixedJoint>());
            CompStatus = ComponentStatus.NotInserted;

            // Необходимо для включения столкновений коллайдеров
            RemoveConnectedPart(checkCollider.transform.parent.GameObject());

            foreach (var screw in screwPoints) screw.SetUnavailable();
            if (checkCollider.TryGetComponent<ConnectionPoint>(out var conPoint))
            {
                conPoint.OnDisconnect();
                SetPCBuildRef(null);
            }

            // Необходимо для разблокировки процессора при отключении кулера
            AttachObject connectTo = checkCollider.GetComponentInParent<AttachObject>();
            if (objectInfo.ComponentType == ComponentType.CPU)
            {
                connectTo.cpuConnected = null;
            }
            if (objectInfo.ComponentType == ComponentType.Cooler && connectTo.cpuConnected)
            {
                connectTo.cpuConnected.GetComponent<XRGrabInteractable>().enabled = true;
            }

            OnDisconnectEvents.Invoke();
        }
    }

    public void SetPCBuildRef(PCBuildManager refObj)
    {
        if (refObj != null)
        {
            PCBuildRef = refObj;
            PCBuildRef.OnDeviceConnected(gameObject);
            foreach (var point in conPoints)
            {
                if (point.ConnectedDevice != null)
                {
                    point.ConnectedDevice.GetComponent<AttachObject>().SetPCBuildRef(refObj);
                }
            }
        }
        else if (PCBuildRef != null)
        {
            PCBuildRef.OnDeviceDisconnected(gameObject);
            PCBuildRef = null;
            foreach (var point in conPoints)
            {
                if (point.ConnectedDevice != null)
                {
                    point.ConnectedDevice.GetComponent<AttachObject>().SetPCBuildRef(null);
                }
            }
        }
    }

    // Методы для подсветки места подключения
    IEnumerator CreateHighlight()
    {
        yield return null;
        
        _highlightParent = new GameObject(name + "_highlight");
        _currentMatForHightlight = invis;
        
        _highlightParent.transform.SetParent(attachPoint.transform);
        _highlightParent.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _highlightParent.transform.localScale = Vector3.one;
        
        _highlightParent.SetActive(false);
        
        foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (meshFilter.sharedMesh == null) continue;

            GameObject newObj = new(meshFilter.name + "_highlight");

            Vector3 worldScale = meshFilter.transform.lossyScale;
            newObj.transform.SetParent(_highlightParent.transform);
            newObj.transform.SetPositionAndRotation(
                meshFilter.transform.position, 
                meshFilter.transform.rotation
            );

            Vector3 parentScale = _highlightParent.transform.lossyScale;
            newObj.transform.localScale = new Vector3(
                worldScale.x / parentScale.x,
                worldScale.y / parentScale.y,
                worldScale.z / parentScale.z
            );

            MeshFilter newFilter = newObj.AddComponent<MeshFilter>();
            newFilter.sharedMesh = meshFilter.sharedMesh;
            
            MeshRenderer newRenderer = newObj.AddComponent<MeshRenderer>();
            int materialsCount = newFilter.sharedMesh.subMeshCount;
            Material[] materials = new Material[materialsCount];
            for (int i = 0; i < materialsCount; i++)
            {
                materials[i] = invis;
            }
            newRenderer.materials = materials;
            
            yield return null;
        }
    }

    void StartHighlight(Collider collider)
    {
        _highlightParent.transform.SetParent(collider.transform);
        _highlightParent.transform.localPosition = new Vector3(0f, 0f, 0f);
        _highlightParent.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        _highlightParent.SetActive(true);
        _currentColliderForHighlight = collider;
    }

    void ChangeHighlightColor(Material mat)
    {
        foreach(MeshRenderer highlightMesh in _highlightParent.GetComponentsInChildren<MeshRenderer>())
        {
            Material[] materials = highlightMesh.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = mat;
            }
            highlightMesh.materials = materials;
        }
        _currentMatForHightlight = mat;
    }

    void EndHighlight()
    {
        _highlightParent.SetActive(false);
        _highlightParent.transform.SetParent(gameObject.transform);
        _currentColliderForHighlight = null;
    }

    //Удаление подсветки вместе с самим объектом
    void OnDestroy()
    {
        Destroy(_highlightParent);
        if (ObjIsAttached) TryUnattach();
    }

    // Метод проверки возможности подключения объекта к разъёму (нужный ли разъём и совместим ли объект с ним)
    void OnTriggerEnter(Collider collider)
    {
        if (ObjIsAttached)
            return;

        if (attachPoint != collider.gameObject)
        {
            if (!collider.gameObject.CompareTag(tag)) // является ли разъём подходящим для подключения
            {
                return;
            }

            StartHighlight(collider);
            checkCollider = null;
            ItemCommon colliderInfo = collider.gameObject.GetComponentInParent<ItemCommon>(); // место, где хранится информация об комплектующем, к которому мы подключаемся
            if (colliderInfo == null)
            {
                checkCollider = collider;
                return;
            }

            switch (objectInfo.ComponentType) //тип комплектующего, который мы подключаем
            {
                case ComponentType.NotSelected:
                    return;
                case ComponentType.CPU:
                    if (objectInfo.GetCPUInfo().SocketType != colliderInfo.GetMotherboardInfo().SocketType) // берём соответствующую информацию об объектах и сравниваем
                    {
                        ChangeHighlightColor(wrong); // показываем знак несовместимости
                        return;
                    }
                    break;
                case ComponentType.RAM:
                    if (objectInfo.GetRAMInfo().DDRType != colliderInfo.GetMotherboardInfo().DDRType)
                    {
                        ChangeHighlightColor(wrong);
                        return;
                    }
                    break;
                case ComponentType.Cooler:
                    if (!objectInfo.GetCoolerInfo().SupportSockets.Contains(colliderInfo.GetMotherboardInfo().SocketType))
                    {
                        ChangeHighlightColor(wrong);
                        return;
                    }
                    break;
                default:
                    break;
            }
            // в случае прохождения проверки запоминаем объект и начинаем отслеживать положения нашего объекта для подсветки верености подключения компонента в разъём
            checkCollider = collider;
        }
    }

    // Метод, управляющий подсветкой места подключения
    void OnTriggerStay(Collider collider)
    {
        if (_highlightParent == null || _currentMatForHightlight == wrong)
            return;
        
        if (ObjIsAttached || interactor == null)
        {
            if (_currentMatForHightlight != invis)
                ChangeHighlightColor(invis);
        } else if (checkCollider != null && Quaternion.Angle(attachPoint.transform.rotation, checkCollider.gameObject.transform.rotation) <= 40)
        {
            if (_currentMatForHightlight != correct)
                ChangeHighlightColor(correct);
        } else if (checkCollider != null)
        {
            if (_currentMatForHightlight != show)
                ChangeHighlightColor(show);
        }
    }

    // Метод, останавливающий подсветку и отслеживание объекта для подключения
    void OnTriggerExit(Collider collider)
    {
        if (collider == _currentColliderForHighlight) EndHighlight();

        if (checkCollider != null && !ObjIsAttached && checkCollider.gameObject == collider.gameObject)
        {
            checkCollider = null;
        }
    }
}
