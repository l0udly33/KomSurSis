using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Gpu
{
    public int Id { get; set; }

    public int Memory { get; set; }

    public string MemoryType { get; set; } = null!;

    public int MemoryFrequency { get; set; }

    public string Connection { get; set; } = null!;

    public int RamQuantity { get; set; }

    public string RamType { get; set; } = null!;

    public int Power { get; set; }

    public string Dimensions { get; set; } = null!;

    public virtual ComputerPart IdNavigation { get; set; } = null!;
}
