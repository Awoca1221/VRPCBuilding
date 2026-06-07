using UnityEngine;

[CreateAssetMenu(fileName = "NewGPUInfo", menuName = "ScriptableObjects/DeviceInfo/GPUInfo", order = 3)]
public class GPUInfo : DeviceInfo
{
    public override ComponentType ComponentType => ComponentType.GPU;
    [IncludeInDict][field: SerializeField] public GPUManufacturer GPUManufacturer { get; private set; } = GPUManufacturer.NotSelected;
    [IncludeInDict][field: SerializeField] public string Model { get; private set; } = "";
    [IncludeInDict][field: SerializeField] public uint MemoryAmountGB { get; private set; } = 0;
    [IncludeInDict] public PCIEType PCIESupport { get; private set; } = PCIEType.NotSelected;
    [IncludeInDict][field: SerializeField] public uint Performance { get; private set; } = 0;
    [IncludeInDict][field: SerializeField] public uint TDP { get; private set; } = 0;
}
