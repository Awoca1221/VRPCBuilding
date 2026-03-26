using UnityEngine;

[CreateAssetMenu(fileName = "NewCoolerInfo", menuName = "ScriptableObjects/DeviceInfo/CoolerInfo", order = 1)]
public class CoolerInfo : DeviceInfo
{
    public override ComponentType ComponentType => ComponentType.Cooler;
    [IncludeInDict][field: SerializeField] public CPUSocketType[] SupportSockets { get; private set; } = new CPUSocketType[0];
    [IncludeInDict][field: SerializeField] public uint TDPLimit { get; private set; } = 0;
}
