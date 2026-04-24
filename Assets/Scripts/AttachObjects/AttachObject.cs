using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public abstract class AttachObject : MonoBehaviour
{
    public GameObject attachPoint;
    public Material correct;
    public Material show;
    public Material wrong;

    protected Material invis;
    protected XRInteractionManager interactionManager;
    protected IXRSelectInteractor interactor;
    protected XRGrabInteractable interactable;
    protected Collider checkCollider;
    public bool objIsAttached { get; protected set; }
    protected GameObject _highlightParent = null;
    protected Material _currentMatForHightlight = null;
    protected Collider _currentColliderForHighlight = null;

    [field: SerializeField] public CheckMultipleConnections MultipleConnections { get; set; } = null;
    [field: SerializeField] public UnityEvent OnConnectEvents { get; set; } = null;
    [field: SerializeField] public UnityEvent OnDisconnectEvents { get; set; } = null;

    protected virtual void Start()
    {
        interactable = GetComponent<XRGrabInteractable>();
        interactionManager = GameObject.Find("XR Interaction Manager").GetComponent<XRInteractionManager>();

        // Создать модель для выделения места подключения
        StartCoroutine(CreateHighlight());

        // Отслеживание нажатия кнопки для подключения и отключения объекта
        interactable.selectEntered.AddListener(OnGrabEnter);
        interactable.selectExited.AddListener(OnGrabExit);

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

    // Методы для отслеживания задач с множественным подключением (оперативная память)
    protected void MultipleConOnConnect()
    {
        MultipleConnections.ConnectObject(gameObject);
    }

    protected void MultipleConOnDisconnect()
    {
        MultipleConnections.DisconnectObject(gameObject);
    }

    // Методы для настройки отслеживания подключения/отключения объекта через триггер
    protected void OnGrabEnter(SelectEnterEventArgs args)
    {
        interactor = args.interactorObject;
        if (interactor.transform.TryGetComponent<HandInfo>(out var info))
        {
            info.activateAction.performed += TryActivateAction;
        }
    }

    protected virtual void OnGrabExit(SelectExitEventArgs args)
    {
        if (interactor.transform.TryGetComponent<HandInfo>(out var info))
        {
            info.activateAction.performed -= TryActivateAction;
        }
        interactor = null;
    }

    // Активация кнопки подключения/отключения объекта
    protected void TryActivateAction(InputAction.CallbackContext context)
    {
        if (objIsAttached)
            TryUnattach();
        else
            TryAttach();
    }

    protected abstract void TryAttach(bool forced = false);

    public abstract void TryUnattach(bool forced = false);

    // Методы для подсветки места подключения
    protected IEnumerator CreateHighlight()
    {
        yield return null;
        
        _highlightParent = new GameObject(name + "_highlight");
        _currentMatForHightlight = invis;
        
        _highlightParent.transform.SetParent(attachPoint.transform);
        _highlightParent.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _highlightParent.transform.SetParent(null);
        _highlightParent.transform.localScale = Vector3.one;
        
        _highlightParent.SetActive(false);
        
        foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (meshFilter.sharedMesh == null) continue;

            GameObject newObj = new(meshFilter.name + "_highlight");

            newObj.transform.localScale = meshFilter.transform.lossyScale;
            newObj.transform.SetParent(_highlightParent.transform);
            newObj.transform.SetPositionAndRotation(
                meshFilter.transform.position, 
                meshFilter.transform.rotation
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
        }
        _highlightParent.transform.SetParent(gameObject.transform);
    }

    protected void StartHighlight(Collider collider)
    {
        if (_highlightParent == null)
        {
            Debug.Log("_highlightParent не успел инициализироваться");
            return;
        }
        _highlightParent.transform.SetParent(collider.transform);
        _highlightParent.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _highlightParent.SetActive(true);
        _currentColliderForHighlight = collider;
    }

    protected void ChangeHighlightColor(Material mat)
    {
        if (_highlightParent == null)
        {
            Debug.Log("_highlightParent не успел инициализироваться");
            return;
        }
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

    protected void EndHighlight()
    {
        if (_highlightParent == null)
        {
            Debug.Log("_highlightParent не успел инициализироваться");
            return;
        }
        _highlightParent.SetActive(false);
        _highlightParent.transform.SetParent(gameObject.transform);
        _currentColliderForHighlight = null;
    }

    //Удаление подсветки вместе с самим объектом
    protected virtual void OnDestroy()
    {
        Destroy(_highlightParent);
        if (objIsAttached) TryUnattach();
    }

    protected abstract void OnTriggerEnter(Collider collider);

    protected abstract void OnTriggerStay(Collider collider);

    protected abstract void OnTriggerExit(Collider collider);
}
