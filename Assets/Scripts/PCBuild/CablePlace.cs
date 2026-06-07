using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HPhysic;
using UnityEngine;

public class CablePlace : MonoBehaviour
{
    [Serializable]
    public class ConnectCableAtStart
    {
        public PhysicCable cable;
        public GameObject startDevice;
        public GameObject endDevice;
    }

    public List<Transform> keyPoints = new();
    public ConnectCableAtStart setupAtStart;

    private readonly List<float> distancesBetweenPoints = new();

    void Start()
    {
        for (int i = 1; i < keyPoints.Count; i++)
        {
            distancesBetweenPoints.Add(Vector3.Distance(keyPoints[i-1].position, keyPoints[i].position));
        }
        
        if (setupAtStart.cable != null)
            StartCoroutine(ConnectCable(setupAtStart.cable, setupAtStart.startDevice, setupAtStart.endDevice));
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (keyPoints.Count == 0 || keyPoints[0] == null) return;
        for (int i = 1; i < keyPoints.Count; i++)
        {
            if (keyPoints[i] == null) return;
            Gizmos.DrawLine(keyPoints[i-1].position, keyPoints[i].position);
        }
    }

    // startDevice - isRequired=false ; endDevice - isRequired=true
    public IEnumerator ConnectCable(PhysicCable cable, GameObject startDevice, GameObject endDevice)
    {
        //for (int i = 0; i < 5; i++)
        yield return new WaitForFixedUpdate();
        
        List<Transform> cablePoints = cable.GetPoints;
        
        if (cablePoints[^1].TryGetComponent<AttachObjectCable>(out var attachEnd))
        {
            attachEnd.ForceAttach(endDevice);
        } else {
            Debug.Log($"Объект {cablePoints[^1].name} не имеет AttachObjectCable");
        }

        if (cablePoints[0].TryGetComponent<AttachObjectCable>(out var attachStart))
        {
            attachStart.ForceAttach(startDevice);
        } else {
            Debug.Log($"Объект {cablePoints[0].name} не имеет AttachObjectCable");
        }

        List<float> distances = new();
        float totalLength;
        
        distances.Add(Vector3.Distance(cablePoints[0].position, keyPoints[0].position));
        distances.AddRange(distancesBetweenPoints);
        distances.Add(Vector3.Distance(keyPoints[^1].position, cablePoints[^1].position));
        totalLength = distances.Sum();

        float spacing = totalLength / cablePoints.Count;
        float targetDist = 0f;

        
        foreach (var cablePoint in cablePoints)
        {
            // === Находим позицию на ломаной линии на расстоянии targetDist ===
            float accumulated;
            Vector3 targetPos = endDevice.transform.position; // запасной вариант
            
            // 1. Первый сегмент: startDevice -> первый keyPoint
            if (targetDist <= distances[0] + Mathf.Epsilon)
            {
                float t = Mathf.InverseLerp(0, distances[0], targetDist);
                targetPos = Vector3.Lerp(cablePoints[0].position, keyPoints[0].position, t);
            } else {
                accumulated = distances[0];
                bool positionFound = false;
                
                // 2. Промежуточные сегменты: между keyPoints
                for (int seg = 0; seg < distancesBetweenPoints.Count; seg++)
                {
                    float segLen = distancesBetweenPoints[seg];
                    if (targetDist <= accumulated + segLen + Mathf.Epsilon)
                    {
                        float t = Mathf.InverseLerp(0, segLen, targetDist - accumulated);
                        targetPos = Vector3.Lerp(keyPoints[seg].position, keyPoints[seg + 1].position, t);
                        positionFound = true;
                        break;
                    }
                    accumulated += segLen;
                }
                
                // 3. Последний сегмент: последний keyPoint -> endDevice
                if (!positionFound)
                {
                    float lastSegLen = distances[^1];
                    float t = Mathf.InverseLerp(0, lastSegLen, targetDist - accumulated);
                    targetPos = Vector3.Lerp(keyPoints[^1].position, cablePoints[^1].position, Mathf.Clamp01(t));
                }
            }
            // Для тестирования
            cablePoint.GetComponent<Rigidbody>().isKinematic = true;
            // === Просто двигаем точку кабеля ===
            cablePoint.SetPositionAndRotation(targetPos, cablePoint.rotation);
            // === Подготовка к следующей итерации ===
            targetDist += spacing;
        }

        yield return new WaitForFixedUpdate();
        foreach (var cablePoint in cablePoints)
            cablePoint.GetComponent<Rigidbody>().isKinematic = false;
    }
}
