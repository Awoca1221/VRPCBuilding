using UnityEngine;
using UnityEngine.Events;

public class ScrewPoint : MonoBehaviour
{
    public UnityAction onStatusChanged;
    public bool IsSecured { get; private set; } = false;
    
    public void SetScrewSecured()
    {
        IsSecured = true;
        onStatusChanged.Invoke();
    }

    public void SetScrewUnsecured()
    {
        IsSecured = false;
        onStatusChanged.Invoke();
    }
}
