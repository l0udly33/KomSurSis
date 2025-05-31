using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Order
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public double Sum { get; set; }

    public int FkState { get; set; }

    public int FkCart { get; set; }

    public int FkClient { get; set; }

    public virtual Feedback? Feedback { get; set; }

    public virtual Cart FkCartNavigation { get; set; } = null!;

    public virtual Client FkClientNavigation { get; set; } = null!;

    public virtual State FkStateNavigation { get; set; } = null!;

    public virtual Payment? Payment { get; set; }
}
