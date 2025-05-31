using System;
using System.Collections.Generic;

namespace ComputerBuildSystem.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual Admin? Admin { get; set; }

    public virtual Client? Client { get; set; }
}
