using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DestroyArea : MonoBehaviour
{
    [Tooltip("BoxCollider, задающий область для проверки")]
    public BoxCollider area;

    public void DestroyDevices()
    {
        Vector3 center = area.transform.TransformPoint(area.center);
        Vector3 size = Vector3.Scale(area.size, area.transform.lossyScale);

        Collider[] colliders = Physics.OverlapBox(center, size / 2f, area.transform.rotation, -1, QueryTriggerInteraction.Ignore);
        HashSet<GameObject> targets = new();

        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb != null && rb.GetComponent<AttachObjectDevice>() != null)
            {
                targets.Add(rb.gameObject);
            }
        }

        // Удаляем найденные объекты
        foreach (var obj in targets)
        {
            Destroy(obj);
        }
    }
}
