using UnityEngine;

[CreateAssetMenu(fileName = "NewRAMInfo", menuName = "ScriptableObjects/DeviceInfo/RAMInfo", order = 4)]
public class RAMInfo : DeviceInfo
{
    public override ComponentType ComponentType => ComponentType.RAM;
    [IncludeInDict][field: SerializeField] public MemoryType DDRType { get; private set; } = MemoryType.NotSelected;
    [IncludeInDict][field: SerializeField] public uint MemoryAmountGB { get; private set; } = 0;
    [IncludeInDict][field: SerializeField] public uint FrequencyMhz { get; private set; } = 0;
}
