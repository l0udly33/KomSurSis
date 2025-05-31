using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Cart
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public double Sum { get; set; }

    public int FkClient { get; set; }

    public virtual ICollection<CartElement> CartElements { get; set; } = new List<CartElement>();

    public virtual Client FkClientNavigation { get; set; } = null!;

    public virtual Order? Order { get; set; }
}
