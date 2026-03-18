using UnityEngine;

[CreateAssetMenu(fileName = "NewStorageDeviceInfo", menuName = "ScriptableObjects/DeviceInfo/StorageDeviceInfo", order = 7)]
public class StorageDeviceInfo2 : DeviceInfo
{
    public override ComponentType ComponentType => ComponentType.StorageDevice;
    [IncludeInDict][field: SerializeField] public StorageDeviceType StorageDeviceType { get; private set; } = StorageDeviceType.NotSelected;
    [IncludeInDict][field: SerializeField] public uint MemoryAmountGB { get; private set; } = 0;
}
