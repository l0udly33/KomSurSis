using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Ram
{
    public int Id { get; set; }

    public string Type { get; set; } = null!;

    public int Frequency { get; set; }

    public int Voltage { get; set; }

    public int Amount { get; set; }

    public virtual ComputerPart IdNavigation { get; set; } = null!;
}
