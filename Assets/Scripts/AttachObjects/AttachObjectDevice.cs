using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(ItemCommon))]
public class AttachObjectDevice : AttachObject
{
    public enum Status
    {
        NotInserted,     // Не вставлен
        InsertedLoose,   // Вставлен, но не закреплён
        FullySecured     // Полностью подключён (все винты)
    }
    public Status DeviceStatus { get; private set; } = Status.NotInserted;
    public PCBuildManager PCBuildRef { get; private set; } = null;

    private ConnectionPoint[] conPoints;
    private SetupPoint[] setupPoints;
    private ItemCommon objectInfo;
    [field: NonSerialized] public GameObject cpuConnected;
    private HashSet<GameObject> _connectedParts = new();

    //Debug.Log("...");
    protected override void Start()
    {
        base.Start();

        conPoints = GetComponentsInChildren<ConnectionPoint>();
        setupPoints = GetComponentsInChildren<SetupPoint>();
        foreach (var point in setupPoints) point.onStatusChanged += OnPointsChangeStatus;
        objectInfo = GetComponentInParent<ItemCommon>();
        interactionManager = GameObject.Find("XR Interaction Manager").GetComponent<XRInteractionManager>();
    }

    private void OnPointsChangeStatus()
    {
        SetupPoint[] screwPoints = setupPoints.Where(point => point.pointType == SetupPoint.Type.Screw).ToArray();
        if (screwPoints.Any(s => s.IsSecured))
        {
            interactable.enabled = false;
        }
        else
        {
            interactable.enabled = true;
        }
        
        if (!objIsAttached) return;

        SetupPoint[] reqPoints = setupPoints.Where(point => point.isRequired).ToArray();
        if (reqPoints.All(r => r.IsSecured))
        {
            DeviceStatus = Status.FullySecured;
        }
        else
        {
            DeviceStatus = Status.InsertedLoose;
        }
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

    // Подключение объекта
    protected override void TryAttach()
    {
        if (checkCollider != null && Quaternion.Angle(attachPoint.transform.rotation, checkCollider.gameObject.transform.rotation) <= 40)
        {
            // Сохранение прошлой иерархии и смена на новую
            Transform oldPlace = transform.parent;
            attachPoint.transform.SetParent(checkCollider.gameObject.transform);
            transform.SetParent(attachPoint.transform);

            // Сдвиг объекта на место подключения
            attachPoint.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            // Возврат к прошлой иерархии
            transform.SetParent(oldPlace);
            attachPoint.transform.SetParent(transform);

            // Скрепление объекта с разъёмом через FixedJoint
            transform.AddComponent<FixedJoint>();
            transform.GetComponent<FixedJoint>().connectedBody = checkCollider.GetComponentInParent<Rigidbody>();

            checkCollider.tag = "Unavailable";
            DeviceStatus = Status.InsertedLoose;
            objIsAttached = true;
            if (interactor != null)
                interactionManager.SelectExit(interactor, interactable);
            
            // Необходимо для отключения столкновений коллайдеров
            AddConnectedPart(checkCollider.transform.parent.GameObject());

            foreach (var point in setupPoints)
            {
                if (point.pointType == SetupPoint.Type.Screw) point.SetAvailable();
            }
            if (checkCollider.TryGetComponent<ConnectionPoint>(out var conPoint))
            {
                conPoint.OnConnect(gameObject);
                Transform conPointParent = conPoint.transform.parent;
                if (conPointParent.TryGetComponent<PCBuildManager>(out var pcBuildRef))
                {
                    SetPCBuildRef(pcBuildRef);
                }
                else if (conPointParent.TryGetComponent<AttachObjectDevice>(out var attachDevice))
                {
                    SetPCBuildRef(attachDevice.PCBuildRef);
                }
            }

            // Необходимо для блокировки процессора при подключении кулера
            AttachObjectDevice connectTo = checkCollider.GetComponentInParent<AttachObjectDevice>();
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
    public override void TryUnattach()
    {
        if (objIsAttached)
        {
            if (checkCollider != null) checkCollider.tag = tag;
            Destroy(GetComponent<FixedJoint>());
            DeviceStatus = Status.NotInserted;
            objIsAttached = false;

            // Необходимо для включения столкновений коллайдеров
            RemoveConnectedPart(checkCollider.transform.parent.GameObject());

            foreach (var point in setupPoints)
            {
                if (point.pointType == SetupPoint.Type.Screw) point.SetUnavailable();
            }
            if (checkCollider.TryGetComponent<ConnectionPoint>(out var conPoint))
            {
                conPoint.OnDisconnect();
                SetPCBuildRef(null);
            }

            // Необходимо для разблокировки процессора при отключении кулера
            AttachObjectDevice connectTo = checkCollider.GetComponentInParent<AttachObjectDevice>();
            if (connectTo == null)
            {
                OnDisconnectEvents.Invoke();
                return;
            }
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
                    point.ConnectedDevice.GetComponent<AttachObjectDevice>().SetPCBuildRef(refObj);
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
                    point.ConnectedDevice.GetComponent<AttachObjectDevice>().SetPCBuildRef(null);
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        Destroy(_highlightParent);
        if (objIsAttached)
        {
            TryUnattach();
            foreach (var point in conPoints)
            {
                if (point.ConnectedDevice != null)
                {
                    point.ConnectedDevice.GetComponent<AttachObject>().TryUnattach();
                }
            }
        }
    }

    // Метод проверки возможности подключения объекта к разъёму (нужный ли разъём и совместим ли объект с ним)
    protected override void OnTriggerEnter(Collider collider)
    {
        if (objIsAttached) return;

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

    // Метод, управляющий подсветкой места подключения
    protected override void OnTriggerStay(Collider collider)
    {
        if (_highlightParent == null || _currentMatForHightlight == wrong)
            return;
        
        if (objIsAttached || interactor == null)
        {
            if (_currentMatForHightlight != invis)
                ChangeHighlightColor(invis);
        }
        else if (checkCollider != null && Quaternion.Angle(attachPoint.transform.rotation, checkCollider.gameObject.transform.rotation) <= 40)
        {
            if (_currentMatForHightlight != correct)
                ChangeHighlightColor(correct);
        }
        else if (checkCollider != null)
        {
            if (_currentMatForHightlight != show)
                ChangeHighlightColor(show);
        }
    }

    // Метод, останавливающий подсветку и отслеживание объекта для подключения
    protected override void OnTriggerExit(Collider collider)
    {
        if (collider == _currentColliderForHighlight) EndHighlight();

        if (checkCollider != null && !objIsAttached && checkCollider.gameObject == collider.gameObject)
        {
            checkCollider = null;
        }
    }
}
