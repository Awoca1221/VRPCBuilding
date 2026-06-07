using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MoveToOrigin : MonoBehaviour
{
    [Tooltip("Время возврата в начало координат")]
    public float homeTime = 1f;
    [SerializeField] private AnimationCurve easeCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool activeOnStart = false;
    private Coroutine homeCoroutine;

    void Start()
    {
        if (TryGetComponent(out XRGrabInteractable grabInteractable))
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
        if (activeOnStart)
            homeCoroutine = StartCoroutine(GoHome());
    }

    private void OnGrabbed(SelectEnterEventArgs _)
    {
        if (homeCoroutine != null)
        {
            StopCoroutine(homeCoroutine);
            homeCoroutine = null;
        }
    }

    private void OnReleased(SelectExitEventArgs _)
    {
        if (gameObject.activeInHierarchy)
            homeCoroutine = StartCoroutine(GoHome());
    }

    void OnDestroy()
    {
        if (homeCoroutine != null)
            StopCoroutine(homeCoroutine);
    }

    private IEnumerator GoHome()
    {
        transform.GetLocalPositionAndRotation(out Vector3 startPos, out Quaternion startRot);
        float elapsed = 0;

        while (elapsed < homeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / homeTime;
            float easedT = easeCurve.Evaluate(t);

            transform.localPosition = Vector3.Lerp(startPos, Vector3.zero, easedT);
            transform.localRotation = Quaternion.Lerp(startRot, Quaternion.identity, easedT);

            yield return null;
        }

        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        homeCoroutine = null;
    }
}
