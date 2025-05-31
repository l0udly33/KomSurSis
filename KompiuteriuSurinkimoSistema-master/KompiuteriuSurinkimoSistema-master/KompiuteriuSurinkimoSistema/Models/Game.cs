using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class Game
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime ReleaseDate { get; set; }

    public string Description { get; set; } = null!;

    public string Developer { get; set; } = null!;

    public virtual ICollection<Genre> FkGenres { get; set; } = new List<Genre>();
}
