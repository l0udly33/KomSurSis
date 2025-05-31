using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Psu
{
    public int Id { get; set; }

    public int Power { get; set; }

    public string SizeStandart { get; set; } = null!;

    public virtual ComputerPart IdNavigation { get; set; } = null!;

    public virtual ICollection<Specification> Specifications { get; set; } = new List<Specification>();
}
