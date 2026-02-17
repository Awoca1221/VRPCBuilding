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

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable), typeof(ItemCommon))]
[RequireComponent(typeof(Outline))]
public class AttachObject : MonoBehaviour
{
    public GameObject attachPoint;
    public Transform parent;
    private Material invis;
    public Material correct;
    public Material wrong;

    private XRInteractionManager interactionManager;
    private IXRSelectInteractor interactor;
    private XRGrabInteractable interactable;
    private Collider checkCollider;
    private Vector3 saveScale;
    private bool ObjIsAttached = false;
    private ItemCommon objectInfo;
    [field: NonSerialized] public GameObject cpuConnected;
    private HashSet<GameObject> _connectedParts = new();
    private GameObject _highlightParent = null;
    private Material _currentMatForHightlight = null;

    [field: SerializeField] public CheckMultipleConnections MultipleConnections { get; set; } = null;
    [field: SerializeField] public UnityEvent OnConnectEvents { get; set; } = null;
    [field: SerializeField] public UnityEvent OnDisconnectEvents { get; set; } = null;
    [field: NonSerialized] private GameObject incompatibleInfo { get; set; } = null;

    //Debug.Log("...");
    void Start()
    {
        GetComponent<Outline>().enabled = false;
        interactable = GetComponent<XRGrabInteractable>();
        objectInfo = GetComponentInParent<ItemCommon>();
        interactionManager = GameObject.Find("XR Interaction Manager").GetComponent<XRInteractionManager>();

        // Создать модель для выделения места подключения
        StartCoroutine(CreateHighlight());

        // Отслеживание нажатия кнопки для подключения и отключения объекта
        interactable.selectEntered.AddListener(OnGrabEnter);
        interactable.selectExited.AddListener(OnGrabExit);

        if (parent) 
            saveScale = parent.localScale;
        else
            saveScale = attachPoint.transform.localScale;
        if (MultipleConnections)
        {
            OnConnectEvents.AddListener(MultipleConOnConnect);
            OnDisconnectEvents.AddListener(MultipleConOnDisconnect);
        }

        GameObject[] gameObjects;
        gameObjects = GameObject.FindGameObjectsWithTag("incmp");

        if (gameObjects.Length == 0)
        {
            Debug.Log("No GameObjects are tagged with 'incmp'");
        } else
        {
            incompatibleInfo = gameObjects[0].transform.GetChild(0).gameObject;
        }

        invis = Resources.Load<Material>("Materials/Invis");
        if (correct == null)
        {
            correct = Resources.Load<Material>("Materials/Correct");
        }
        if (wrong == null)
        {
            wrong = Resources.Load<Material>("Materials/Wrong");
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
            Transform oldPlace;

            if (parent)
            {
                oldPlace = parent.parent;
                attachPoint.transform.SetParent(checkCollider.gameObject.transform, true);
                parent.SetParent(attachPoint.transform, true);
            } else
            {
                oldPlace = attachPoint.transform.parent;
                attachPoint.transform.SetParent(checkCollider.gameObject.transform, true);
            }

            attachPoint.transform.localPosition = new Vector3(0f, 0f, 0f);
            attachPoint.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);

            if (parent)
            {
                parent.SetParent(oldPlace, true);
                attachPoint.transform.SetParent(parent, true);
                parent.localScale = saveScale;
            } else
            {
                attachPoint.transform.SetParent(oldPlace, true);
            }
            attachPoint.transform.localScale = saveScale;

            if (parent)
            {
                parent.AddComponent<FixedJoint>();
                parent.GetComponent<FixedJoint>().connectedBody = checkCollider.GetComponentInParent<Rigidbody>();
            } else
            {
                attachPoint.AddComponent<FixedJoint>();
                attachPoint.GetComponent<FixedJoint>().connectedBody = checkCollider.GetComponentInParent<Rigidbody>();
            }
            checkCollider.tag = "Unavailable";
            ObjIsAttached = true;
            if (interactor != null)
                interactionManager.SelectExit(interactor, interactable);
            // Необходимо для отключения столкновений коллайдеров
            AddConnectedPart(checkCollider.transform.parent.GameObject());
            // Необходимо для блокировки процессора при подключении кулера
            AttachObject connectTo = checkCollider.GetComponentInParent<AttachObject>();
            if (connectTo == null)
            {
                OnConnectEvents.Invoke();
                return;
            }
            if (objectInfo.ComponentType == ComponentType.CPU)
            {
                if (parent)
                    connectTo.cpuConnected = parent.GameObject();
                else
                    connectTo.cpuConnected = attachPoint.GameObject();
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
            if (parent)
                Destroy(parent.GetComponent<FixedJoint>());
            else
                Destroy(attachPoint.GetComponent<FixedJoint>());
            ObjIsAttached = false;
            // Необходимо для включения столкновений коллайдеров
            RemoveConnectedPart(checkCollider.transform.parent.GameObject());
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

    // Методы для подсветки места подключения
    IEnumerator CreateHighlight()
    {
        yield return null;
        _highlightParent = new(name + "_highlight");
        _currentMatForHightlight = invis;
        _highlightParent.SetActive(false);
        _highlightParent.transform.SetParent(attachPoint.transform);
        _highlightParent.transform.localPosition = new Vector3(0f, 0f, 0f);
        _highlightParent.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (meshFilter.sharedMesh == null) continue;

            GameObject newObj = new(meshFilter.name + "_highlight");
            newObj.transform.SetPositionAndRotation(meshFilter.transform.position, meshFilter.transform.rotation);
            newObj.transform.localScale = meshFilter.transform.lossyScale;
            newObj.transform.SetParent(_highlightParent.transform);

            MeshFilter newFilter = newObj.AddComponent<MeshFilter>();
            newFilter.sharedMesh = meshFilter.sharedMesh;
            MeshRenderer newRenderer = newObj.AddComponent<MeshRenderer>();
            int materialsCount = newFilter.mesh.subMeshCount;
            Material[] materials = new Material[materialsCount];
            for (int i = 0; i < materialsCount; i++)
            {
                materials[i] = invis;
            }
            newRenderer.materials = materials;
            yield return null;
        }
    }

    void StartHighlight()
    {
        _highlightParent.transform.SetParent(checkCollider.transform);
        _highlightParent.transform.localPosition = new Vector3(0f, 0f, 0f);
        _highlightParent.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        _highlightParent.SetActive(true);
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
    }

    //Удаление подсветки вместе с самим объектом
    void OnDestroy()
    {
        Destroy(_highlightParent);
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
            ItemCommon colliderInfo = collider.gameObject.GetComponentInParent<ItemCommon>(); // место, где хранится информация об комплектующем, к которому мы подключаемся
            if (colliderInfo == null)
            {
                return;
            }
            switch (objectInfo.ComponentType) //тип комплектующего, который мы подключаем
            {
                case ComponentType.NotSelected:
                    return;
                case ComponentType.CPU:
                    if (objectInfo.GetCPUInfo().SocketType != colliderInfo.GetMotherboardInfo().SocketType) // берём соответствующую информацию об объектах и сравниваем
                    {
                        incompatibleInfo.SetActive(true); // показываем знак несовместимости
                        return;
                    }
                    break;
                case ComponentType.RAM:
                    if (objectInfo.GetRAMInfo().DDRType != colliderInfo.GetMotherboardInfo().DDRType)
                    {
                        incompatibleInfo.SetActive(true);
                        return;
                    }
                    break;
                case ComponentType.Cooler:
                    if (!objectInfo.GetCoolerInfo().SupportSockets.Contains(colliderInfo.GetMotherboardInfo().SocketType))
                    {
                        incompatibleInfo.SetActive(true);
                        return;
                    }
                    break;
                default:
                    break;
            }
            // в случае прохождения проверки запоминаем объект и начинаем отслеживать положения нашего объекта для подсветки верености подключения компонента в разъём
            checkCollider = collider;
            StartHighlight();
        }
    }

    // Метод, управляющий подсветкой места подключения
    void OnTriggerStay(Collider collider)
    {
        if (_highlightParent == null)
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
            if (_currentMatForHightlight != wrong)
                ChangeHighlightColor(wrong);
        }
    }

    // Метод, останавливающий подсветку и отслеживание объекта для подключения
    void OnTriggerExit(Collider collider)
    {
        incompatibleInfo.SetActive(false);
        if (checkCollider != null && !ObjIsAttached && checkCollider.gameObject == collider.gameObject)
        {
            EndHighlight();
            checkCollider = null;
        }
    }
}
