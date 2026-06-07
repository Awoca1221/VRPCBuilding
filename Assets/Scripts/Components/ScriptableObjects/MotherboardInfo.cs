using UnityEngine;

[CreateAssetMenu(fileName = "NewMotherboardInfo", menuName = "ScriptableObjects/DeviceInfo/MotherboardInfo", order = 5)]
public class MotherboardInfo : DeviceInfo
{
    public override ComponentType ComponentType => ComponentType.Motherboard;
    [IncludeInDict][field: SerializeField] public CPUManufacturer CPUManufacturer { get; private set; } = CPUManufacturer.NotSelected;
    [IncludeInDict][field: SerializeField] public CPUSocketType SocketType { get; private set; } = CPUSocketType.NotSelected;
    [IncludeInDict] public PCIEType PCIESupport { get; private set; } = PCIEType.NotSelected;
    [IncludeInDict][field: SerializeField] public MemoryType DDRType { get; private set; } = MemoryType.NotSelected;
}
