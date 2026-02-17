using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ScrewdriverManager : MonoBehaviour
{
    private enum TwistDirection
    {
        Clockwise,
        CClockwise
    }
    public float requiredDegress = 720f;
    //[Range(0f, 0.001f)]
    public float movePerDegree = 1e-05f;
    public Transform screw;
    public Transform screwHighlight;

    private Transform usePlace;
    private Rigidbody rb;
    private bool isInUse = false;
    private TwistDirection direction = TwistDirection.Clockwise;
    private float totalProgress = 0f;
    private IXRSelectInteractor interactor;
    private XRGrabInteractable interactable;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        interactable = GetComponent<XRGrabInteractable>();
        interactable.selectEntered.AddListener(OnGrabEnter);
        interactable.selectExited.AddListener(OnGrabExit);
    }

    // Отслеживает прогресс закручивания
    void Update()
    {
        if (!isInUse) return;

        Vector3 localAngularVel = transform.InverseTransformDirection(rb.angularVelocity);
        float delta = localAngularVel.x * Mathf.Rad2Deg * Time.deltaTime;
        switch (direction)
        {
            case TwistDirection.Clockwise:
                delta *= -1;
                if (delta > 0.1f)
                {
                    totalProgress += delta;
                    transform.Translate(Mathf.Sign(delta) * delta * movePerDegree * Vector3.right);
                }
                break;
            
            case TwistDirection.CClockwise:
                if (delta > 0.1f)
                {
                    totalProgress += delta;
                    transform.Translate(Mathf.Sign(delta) * delta * movePerDegree * Vector3.left);
                }
                break;
        }
        //Debug.Log($"totalProgress:{totalProgress}");
        if (totalProgress >= requiredDegress)
        {
            //Debug.Log("Процесс прошёл успешно");
            switch (direction)
            {
                case TwistDirection.Clockwise:
                    Instantiate(screw.gameObject, usePlace, true);
                    break;
                
                case TwistDirection.CClockwise:
                    Transform oldScrewClone = usePlace.Find($"{screw.name}(Clone)");
                    if (oldScrewClone != null)
                    {
                        Destroy(oldScrewClone.gameObject);
                    }
                    break;
            }
            RemoveInUseState();
        }
    }

    // Подсветка места закручивания
    void StartHighlight()
    {
        screwHighlight.parent = usePlace;
        screwHighlight.SetLocalPositionAndRotation(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f));
        screwHighlight.Translate(requiredDegress * movePerDegree * Vector3.right);
        screwHighlight.GetComponent<MeshRenderer>().enabled = true;
    }

    void EndHighlight()
    {
        screwHighlight.GetComponent<MeshRenderer>().enabled = false;
        screwHighlight.transform.parent = transform;
    }

    // Отслеживание нахождения в месте закручивания
    void OnTriggerEnter(Collider collider)
    {
        if (!collider.gameObject.CompareTag(tag))
        {
            return;
        }
        usePlace = collider.transform;
        StartHighlight();
    }

    void OnTriggerExit(Collider collider)
    {
        if (usePlace != null && collider.gameObject == usePlace.gameObject && !isInUse)
        {
            EndHighlight();
            usePlace = null;
        }
    }

    // Методы для настройки отслеживания использования объекта через триггер
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
        if (isInUse)
        {
            RemoveInUseState();
        }
    }

    // Переход в состояние закручивания и выход из него
    private void SetInUseState()
    {
        rb.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezeRotationX;
        //GetComponent<CapsuleCollider>().enabled = false;
        screw.GetComponent<MeshRenderer>().enabled = true;
        isInUse = true;
        if (usePlace != null)
        {
            EndHighlight();
            Transform oldScrewClone = usePlace.Find($"{screw.name}(Clone)");
            if (oldScrewClone != null)
            {
                oldScrewClone.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    private void RemoveInUseState()
    {
        totalProgress = 0f;
        rb.constraints = RigidbodyConstraints.None;
        //GetComponent<CapsuleCollider>().enabled = true;
        screw.GetComponent<MeshRenderer>().enabled = false;
        isInUse = false;
        if (usePlace != null)
        {
            StartHighlight();
            Transform oldScrewClone = usePlace.Find($"{screw.name}(Clone)");
            if (oldScrewClone != null)
            {
                oldScrewClone.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    // Активация кнопки подключения/отключения объекта
    private void TryActivateAction(InputAction.CallbackContext context)
    {
        if (usePlace != null || isInUse)
        {
            if (isInUse)
            {
                RemoveInUseState();
            } else
            {
                screw.parent = null;
                transform.parent = screw;
                screw.SetPositionAndRotation(usePlace.position, usePlace.rotation);
                if (usePlace.childCount == 1)
                {
                    direction = TwistDirection.Clockwise;
                    screw.Translate(requiredDegress * movePerDegree * Vector3.right);
                } else
                {
                    direction = TwistDirection.CClockwise;
                }
                transform.parent = null;
                screw.parent = transform;
                SetInUseState();
            }
        }
    }
}
