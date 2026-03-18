using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComponentType
{
    NotSelected,
    Cooler,
    CPU,
    GPU,
    RAM,
    Motherboard,
    PowerSupply,
    StorageDevice
}

public enum CPUManufacturer
{
    NotSelected,
    AMD,
    Intel
}

public enum CPUSocketType
{
    NotSelected,
    AM5,
    AM4,
    [InspectorName("AM3+")]
    AM3Plus,
    AM3,
    [InspectorName("LGA 1851")]
    LGA_1851,
    [InspectorName("LGA 1700")]
    LGA_1700,
    [InspectorName("LGA 1200")]
    LGA_1200,
    [InspectorName("LGA 2066")]
    LGA_2066,
    [InspectorName("LGA 1151v2")]
    LGA_1151v2,
    [InspectorName("LGA 1151")]
    LGA_1151,
    [InspectorName("LGA 2011-3")]
    LGA_2011_3,
    [InspectorName("LGA 1150")]
    LGA_1150,
    [InspectorName("LGA 2011")]
    LGA_2011,
    [InspectorName("LGA 1155")]
    LGA_1155,
    [InspectorName("LGA 1156")]
    LGA_1156,
    [InspectorName("LGA 1366")]
    LGA_1366
}

public enum GPUManufacturer
{
    NotSelected,
    Nvidia,
    AMD,
    Intel
}

public enum PCIEType
{
    NotSelected,
    [InspectorName("PCI-E 5.0 x16")]
    PCIE5x16,
    [InspectorName("PCI-E 5.0 x8")]
    PCIE5x8,
    [InspectorName("PCI-E 5.0 x4")]
    PCIE5x4,
    [InspectorName("PCI-E 4.0 x16")]
    PCIE4x16,
    [InspectorName("PCI-E 4.0 x8")]
    PCIE4x8,
    [InspectorName("PCI-E 4.0 x4")]
    PCIE4x4,
    [InspectorName("PCI-E 3.0 x16")]
    PCIE3x16,
    [InspectorName("PCI-E 3.0 x8")]
    PCIE3x8,
    [InspectorName("PCI-E 3.0 x4")]
    PCIE3x4,
}

public enum MemoryType
{
    NotSelected,
    DDR5,
    DDR4,
    DDR3
}

public enum StorageDeviceType
{
    NotSelected,
    SSD,
    HDD
}
