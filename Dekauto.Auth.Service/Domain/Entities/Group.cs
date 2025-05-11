using System;
using System.Collections.Generic;

namespace Dekauto.Auth.Service.Domain.Entities;

public partial class Group
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
