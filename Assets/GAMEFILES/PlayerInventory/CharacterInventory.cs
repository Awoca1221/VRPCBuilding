using System;
using Unity.XR.CoreUtils;
using UnityEngine;

[Serializable]
public class BodySocket
{
    [NonSerialized] public GameObject gameObject = null;
    [Range(0.01f, 1f)]
    public float heightRatio;
    public float positionX;
    public float positionZ;
}

public class CharacterInventory : MonoBehaviour
{
    public XROrigin XROrigin;
    public BodySocket[] bodySockets;
    public BodySocket pistolet;
    public GameObject defaultPrefab;
    public GameObject pistoletPrefab;

    private void Start()
    {
        foreach (var bodySocket in bodySockets)
        {
            if (bodySocket != null)
                bodySocket.gameObject = Instantiate(defaultPrefab, this.transform);
        }
        if (pistoletPrefab != null)
        {
            pistolet.gameObject = Instantiate(pistoletPrefab, this.transform);
        }
    }

    void Update()
    {
        var playerHeight = XROrigin.CameraYOffset;
        var currentHMDRot = XROrigin.Camera.transform.rotation;
        var currentPlayerBodyPos = XROrigin.transform.position;

        foreach (var bodySocket in bodySockets)
        {
            bodySocket.gameObject.transform.localPosition = new Vector3(
                bodySocket.positionX, (playerHeight * bodySocket.heightRatio), bodySocket.positionZ);
        }
        if (pistolet.gameObject != null)
        {
            pistolet.gameObject.transform.localPosition = new Vector3(
                pistolet.positionX, (playerHeight * pistolet.heightRatio), pistolet.positionZ);
        }
        transform.SetPositionAndRotation(
            new Vector3(currentPlayerBodyPos.x, currentPlayerBodyPos.y, currentPlayerBodyPos.z),
            new Quaternion(transform.rotation.x, currentHMDRot.y, transform.rotation.z, currentHMDRot.w)
        );
    }
}
