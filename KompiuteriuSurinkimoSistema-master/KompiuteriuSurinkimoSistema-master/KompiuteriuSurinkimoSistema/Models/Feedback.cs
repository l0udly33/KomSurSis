using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public int Score { get; set; }

    public DateTime Date { get; set; }

    public int FkOrder { get; set; }

    public virtual Order FkOrderNavigation { get; set; } = null!;
}
