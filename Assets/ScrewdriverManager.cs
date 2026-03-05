using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Image = UnityEngine.UI.Image;

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
    public Transform instrumentModel;
    public Transform screw;
    public Transform screwHighlight;
    public Transform UIProgress;
    public Image UIImageProgress;
    public AudioManager soundPoint;
    
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

    void LateUpdate()
    {
        if (!isInUse) return;

        UIProgress.transform.LookAt(Camera.main.transform);
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
                    Vector3 moveDistance = delta * movePerDegree * Vector3.right;
                    transform.Translate(moveDistance);
                    instrumentModel.Translate(moveDistance);
                    instrumentModel.Rotate(delta * Vector3.left);
                    float audVolume = Mathf.Clamp01(delta / 7f);
                    soundPoint.SetVolume(audVolume);
                }
                else
                {
                    soundPoint.SetVolume(0f);
                }
                break;
            
            case TwistDirection.CClockwise:
                if (delta > 0.1f)
                {
                    totalProgress += delta;
                    Vector3 moveDistance = delta * movePerDegree * Vector3.left;
                    transform.Translate(moveDistance);
                    instrumentModel.Translate(moveDistance);
                    instrumentModel.Rotate(delta * Vector3.right);
                    float audVolume = Mathf.Clamp01(delta / 7f);
                    soundPoint.SetVolume(audVolume);
                }
                else
                {
                    soundPoint.SetVolume(0f);
                }
                break;
        }

        float fillAmount = Mathf.Clamp01(totalProgress / requiredDegress);
        UIImageProgress.fillAmount = fillAmount;

        //Debug.Log($"totalProgress:{totalProgress}");
        if (totalProgress >= requiredDegress)
        {
            SetupPoint screwPoint = usePlace.GetComponent<SetupPoint>();
            //Debug.Log("Процесс прошёл успешно");
            switch (direction)
            {
                case TwistDirection.Clockwise:
                    Instantiate(screw.gameObject, usePlace, true);
                    if (screwPoint != null)
                    {
                        screwPoint.SetSecured();
                    }
                    break;
                
                case TwistDirection.CClockwise:
                    Transform oldScrewClone = usePlace.Find($"{screw.name}(Clone)");
                    if (oldScrewClone != null)
                    {
                        Destroy(oldScrewClone.gameObject);
                    }
                    if (screwPoint != null)
                    {
                        screwPoint.SetUnsecured();
                    }
                    break;
            }
            RemoveInUseState();
        }
    }

    // Подсветка места закручивания
    void StartHighlight()
    {
        screwHighlight.SetParent(usePlace);
        screwHighlight.SetLocalPositionAndRotation(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f));
        screwHighlight.Translate(requiredDegress * movePerDegree * Vector3.right);
        screwHighlight.GetComponent<MeshRenderer>().enabled = true;
    }

    void EndHighlight()
    {
        screwHighlight.GetComponent<MeshRenderer>().enabled = false;
        screwHighlight.transform.SetParent(transform);
    }

    // Отслеживание нахождения в месте закручивания
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag(tag) && !isInUse)
        {
            usePlace = collider.transform;
            StartHighlight();
        }
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
        
        screw.SetParent(null);
        transform.SetParent(screw);
        screw.SetPositionAndRotation(usePlace.position, usePlace.rotation);
        if (usePlace.childCount == 1)
        {
            direction = TwistDirection.Clockwise;
            screw.Translate(requiredDegress * movePerDegree * Vector3.right);
        }
        else
        {
            direction = TwistDirection.CClockwise;
        }
        transform.SetParent(null);
        screw.SetParent(instrumentModel);

        screw.GetComponent<MeshRenderer>().enabled = true;
        Vector3 abovePos = transform.position + Vector3.up * 0.025f;
        UIProgress.SetParent(usePlace);
        UIProgress.transform.position = abovePos;
        UIProgress.GetComponent<Canvas>().enabled = true;

        //transform.SetParent(usePlace);
        instrumentModel.SetParent(usePlace);
        soundPoint.SetVolume(0f);
        soundPoint.StartScrewLoopSound();

        EndHighlight();
        isInUse = true;
        Transform oldScrewClone = usePlace.Find($"{screw.name}(Clone)");
        if (oldScrewClone != null)
        {
            oldScrewClone.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private void RemoveInUseState()
    {
        totalProgress = 0f;
        rb.constraints = RigidbodyConstraints.None;
        screw.GetComponent<MeshRenderer>().enabled = false;
        UIProgress.GetComponent<Canvas>().enabled = false;
        UIProgress.SetParent(transform);

        //transform.SetParent(null);
        instrumentModel.SetParent(transform);
        instrumentModel.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        soundPoint.StopSound();

        StartHighlight();
        isInUse = false;
        Transform oldScrewClone = usePlace.Find($"{screw.name}(Clone)");
        if (oldScrewClone != null)
        {
            oldScrewClone.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    // Активация кнопки подключения/отключения объекта
    private void TryActivateAction(InputAction.CallbackContext context)
    {
        if (isInUse)
        {
            RemoveInUseState();
        }
        else if (usePlace != null)
        {
            SetInUseState();
        }
    }
}
