using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Motherboard
{
    public int Id { get; set; }

    public int MaximumMemoryFrequency { get; set; }

    public string MemoryStandart { get; set; } = null!;

    public int MaximumAmountOfMemory { get; set; }

    public string CpuSocket { get; set; } = null!;

    public string GpuSocket { get; set; } = null!;

    public string MemoryConnection { get; set; } = null!;

    public string SizeStandart { get; set; } = null!;

    public virtual ComputerPart IdNavigation { get; set; } = null!;
}
