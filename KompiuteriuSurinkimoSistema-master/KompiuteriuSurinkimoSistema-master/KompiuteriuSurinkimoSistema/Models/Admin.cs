using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Admin
{
    public int IdUser { get; set; }

    public virtual User IdUserNavigation { get; set; } = null!;
}
