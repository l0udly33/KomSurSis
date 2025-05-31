using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class HardDisk
{
    public int Id { get; set; }

    public string Type { get; set; } = null!;

    public string Capacity { get; set; } = null!;

    public int ReadingSpeed { get; set; }

    public int WritingSpeed { get; set; }

    public string Connection { get; set; } = null!;

    public virtual ComputerPart IdNavigation { get; set; } = null!;
}
