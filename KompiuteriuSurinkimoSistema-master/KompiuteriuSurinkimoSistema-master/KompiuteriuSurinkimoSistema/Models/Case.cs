using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Case
{
    public int Id { get; set; }

    public string Standarts { get; set; } = null!;

    public string Dimensions { get; set; } = null!;

    public string Color { get; set; } = null!;

    public virtual ComputerPart IdNavigation { get; set; } = null!;
}
