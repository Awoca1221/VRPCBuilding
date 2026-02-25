using UnityEngine;
using UnityEngine.Events;

public class ScrewPoint : MonoBehaviour
{
    public UnityAction onStatusChanged;
    public bool IsSecured { get; private set; } = false;
    public bool IsAvailable { get; private set; } = false;
    
    public void SetSecured()
    {
        IsSecured = true;
        onStatusChanged.Invoke();
    }

    public void SetUnsecured()
    {
        IsSecured = false;
        onStatusChanged.Invoke();
    }

    public void SetAvailable()
    {
        IsAvailable = true;
    }

    public void SetUnavailable()
    {
        IsAvailable = false;
    }
}
