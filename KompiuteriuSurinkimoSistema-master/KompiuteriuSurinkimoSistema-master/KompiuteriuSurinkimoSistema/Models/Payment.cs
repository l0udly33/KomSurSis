using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Payment
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public double Sum { get; set; }

    public string Iban { get; set; } = null!;

    public int FkOrder { get; set; }

    public virtual Order FkOrderNavigation { get; set; } = null!;
}
