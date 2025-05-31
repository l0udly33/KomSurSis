using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class CartElement
{
    public int Id { get; set; }

    public int Quantity { get; set; }

    public int FkCart { get; set; }

    public int FkComputerPart { get; set; }

    public virtual Cart FkCartNavigation { get; set; } = null!;

    public virtual ComputerPart FkComputerPartNavigation { get; set; } = null!;
}
