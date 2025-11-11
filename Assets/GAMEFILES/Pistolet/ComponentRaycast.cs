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

    private void FireRay(InputAction.CallbackContext context)
    {
        if (isGrabbed)
        {
            RaycastHit? closestValidHit = GetClosestHit();

            if (closestValidHit.HasValue)
            {
                var info = closestValidHit.Value.collider.GetComponent<ItemCommon>().GetInfo().ToDict();
                text.SetText("{" + string.Join(", ", info.Select(kv => $"{kv.Key}: {kv.Value}")) + "}");
                title.SetText("");
            }
            else
            {
                text.SetText("");
                title.SetText("");
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
