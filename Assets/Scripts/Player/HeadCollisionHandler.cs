using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadCollisionHandler : MonoBehaviour
{
    [SerializeField] private HeadCollisionDetector _detector;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] public float pushBackStrength = 1.0f;
    [SerializeField] private FadeEffect _blackScreenFade;
    
    private Transform _bodyTransform;

    void Start()
    {
        _bodyTransform = _characterController.transform;
    }

    private Vector3 CalculatePushBackDirection(List<RaycastHit> colliderHits)
    {
        Vector3 combinedNormal = Vector3.zero;
        foreach (RaycastHit hitPoint in colliderHits)
        {
            combinedNormal +=
                new Vector3(hitPoint.normal.x, 0, hitPoint.normal.z); ;
        }
        return combinedNormal;
    }

    private Vector3 CalculateWallNormal(List<RaycastHit> hits)
    {
        Vector3 combinedNormal = Vector3.zero;
        int validHits = 0;
        
        foreach (RaycastHit hit in hits)
        {
            Vector3 horizontalNormal = new Vector3(hit.normal.x, 0, hit.normal.z).normalized;
            if (horizontalNormal.magnitude > 0.1f)
            {
                combinedNormal += horizontalNormal;
                validHits++;
            }
        }
        
        return validHits > 0 ? combinedNormal / validHits : Vector3.zero;
    }

    private void Update()
    {
        if (_detector.InsideCollider)
        {
            _blackScreenFade.Fade(true);
            return;
        }
        if (_detector.DetectedColliderHits.Count <= 0)
        {
            _blackScreenFade.Fade(false);
            return;
        }

        Vector3 directionToBody = (_bodyTransform.position - transform.position).normalized;
        Vector3 avgWallNormal = CalculateWallNormal(_detector.DetectedColliderHits);
        float dotToBody = Vector3.Dot(avgWallNormal.normalized, directionToBody);

        if (dotToBody > 0.1f) 
        {
            Vector3 pushBackDirection
                = CalculatePushBackDirection(_detector.DetectedColliderHits);

            Debug.DrawRay(transform.position, pushBackDirection.normalized, Color.magenta);

            _characterController
                .Move(pushBackDirection.normalized * pushBackStrength * Time.deltaTime);
            
            Debug.DrawRay(transform.position, directionToBody * 0.3f, Color.green);
            Debug.DrawRay(transform.position, avgWallNormal * 0.3f, Color.red);
        }
    }
}
