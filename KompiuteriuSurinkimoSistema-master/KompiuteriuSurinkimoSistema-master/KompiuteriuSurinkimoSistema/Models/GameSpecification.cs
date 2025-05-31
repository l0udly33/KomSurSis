using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class GameSpecification
{
    public int FkSpecifications { get; set; }

    public int FkGame { get; set; }

    public virtual Specification FkSpecificationsNavigation { get; set; } = null!;
}
