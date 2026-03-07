using UnityEngine;
using UnityEngine.Events;

public class SetupPoint : MonoBehaviour
{
    public enum Type
    {
        NotSelected,
        Screw,
        PowerSocket
    }

    public Type pointType = Type.NotSelected;
    public bool isRequired = true;
    public bool IsSecured { get; private set; } = false;
    public bool IsAvailable { get; private set; } = false;
    public UnityAction onStatusChanged;

    public void SetSecured()
    {
        IsSecured = true;
        onStatusChanged?.Invoke();
    }

    public void SetUnsecured()
    {
        IsSecured = false;
        onStatusChanged?.Invoke();
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
