using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Cpu
{
    public int Id { get; set; }

    public string Memory { get; set; } = null!;

    public string Connection { get; set; } = null!;

    public int Cores { get; set; }

    public int Frequency { get; set; }

    public int Power { get; set; }

    public virtual ComputerPart IdNavigation { get; set; } = null!;
}
