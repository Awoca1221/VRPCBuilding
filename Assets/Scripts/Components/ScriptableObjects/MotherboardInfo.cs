using UnityEngine;

[CreateAssetMenu(fileName = "NewMotherboardInfo", menuName = "ScriptableObjects/DeviceInfo/MotherboardInfo", order = 5)]
public class MotherboardInfo2 : DeviceInfo
{
    public override ComponentType ComponentType => ComponentType.Motherboard;
    [IncludeInDict][field: SerializeField] public CPUManufacturer CPUManufacturer { get; private set; } = CPUManufacturer.NotSelected;
    [IncludeInDict][field: SerializeField] public CPUSocketType SocketType { get; private set; } = CPUSocketType.NotSelected;
    [IncludeInDict][field: SerializeField] public PCIEType PCIESupport { get; private set; } = PCIEType.NotSelected;
    [IncludeInDict][field: SerializeField] public MemoryType DDRType { get; private set; } = MemoryType.NotSelected;
}
