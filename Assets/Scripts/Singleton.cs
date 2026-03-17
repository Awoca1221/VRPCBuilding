using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance;
    
    public static T Instance 
    { 
        get 
        {
            if (instance == null)
            {
                GameObject newInst = new(typeof(T).Name);
                instance = newInst.AddComponent<T>();
                DontDestroyOnLoad(newInst);
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}
