using UnityEngine;

[CreateAssetMenu(fileName = "NewCPUInfo", menuName = "ScriptableObjects/DeviceInfo/CPUInfo", order = 2)]
public class CPUInfo2 : DeviceInfo
{
    public override ComponentType ComponentType => ComponentType.CPU;
    [IncludeInDict][field: SerializeField] public CPUManufacturer CPUManufacturer { get; private set; } = CPUManufacturer.NotSelected;
    [IncludeInDict][field: SerializeField] public string Model { get; private set; } = "";
    [IncludeInDict][field: SerializeField] public CPUSocketType SocketType { get; private set; } = CPUSocketType.NotSelected;
    [IncludeInDict][field: SerializeField] public uint Performance { get; private set; } = 0;
    [IncludeInDict][field: SerializeField] public uint TDP { get; private set; } = 0;
}
