using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Client
{
    public int IdUser { get; set; }

    public int LoyaltyPoints { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual User IdUserNavigation { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
