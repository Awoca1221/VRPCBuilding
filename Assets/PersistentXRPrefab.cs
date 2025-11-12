using UnityEngine;

public class PersistentXRPrefab : MonoBehaviour
{
    private static PersistentXRPrefab instancePrefab;

    void Awake()
    {
        if (instancePrefab == null)
        {
            instancePrefab = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
