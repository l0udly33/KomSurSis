using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Specification
{
    public int Id { get; set; }

    public int FkSpecificationType { get; set; }

    public int FkPsu { get; set; }

    public int FkCpu { get; set; }

    public int FkHardDisk { get; set; }

    public int FkGpu { get; set; }

    public virtual Psu FkPsuNavigation { get; set; } = null!;

    public virtual ICollection<GameSpecification> GameSpecifications { get; set; } = new List<GameSpecification>();
}
