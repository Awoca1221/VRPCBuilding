using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class InstrumentSpawn : MonoBehaviour
{
    [field: SerializeField]
    public GameObject instrumentsPrefab { get; private set; }
    [field: SerializeField]
    public GameObject instrumentsOnScene { get; private set; }

    private List<GameObject> instruments = new();

    void Start()
    {
        if (instrumentsOnScene != null)
        {
            instrumentsOnScene.GetChildGameObjects(instruments);
        }
    }

    public void SpawnInstruments()
    {
        if (instrumentsOnScene != null)
        {
            DestroyInstruments();
        }
        instrumentsOnScene = Instantiate(instrumentsPrefab, transform);
        instrumentsOnScene.GetChildGameObjects(instruments);
    }

    public void DestroyInstruments()
    {
        foreach (var obj in instruments)
        {
            Destroy(obj);
        }
        instruments.Clear();
        Destroy(instrumentsOnScene);
    }
}
