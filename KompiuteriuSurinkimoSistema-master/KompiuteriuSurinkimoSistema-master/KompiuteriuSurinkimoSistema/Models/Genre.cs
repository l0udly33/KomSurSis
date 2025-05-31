using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Genre
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Game> FkGames { get; set; } = new List<Game>();
}
