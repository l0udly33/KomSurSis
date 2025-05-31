using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class ComputerPart
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Manufacturer { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string Type { get; set; } = null!;

    public double Price { get; set; }

    public int Quantity { get; set; }

    public string Description { get; set; } = null!;

    public string? Benchmark { get; set; }

    public virtual ICollection<CartElement> CartElements { get; set; } = new List<CartElement>();

    public virtual Case? Case { get; set; }

    public virtual Cpu? Cpu { get; set; }

    public virtual Gpu? Gpu { get; set; }

    public virtual HardDisk? HardDisk { get; set; }

    public virtual Motherboard? Motherboard { get; set; }

    public virtual Psu? Psu { get; set; }

    public virtual Ram? Ram { get; set; }
}
