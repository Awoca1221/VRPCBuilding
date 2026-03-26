using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerSupplyInfo", menuName = "ScriptableObjects/DeviceInfo/PowerSupplyInfo", order = 6)]
public class PowerSupplyInfo : DeviceInfo
{
    public override ComponentType ComponentType => ComponentType.PowerSupply;
    [IncludeInDict][field: SerializeField] public uint PowerSupplyMaxPower { get; private set; } = 0;
}
