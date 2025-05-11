using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dekauto.Auth.Service.Domain.Entities;

public partial class Role
{
    public Guid Id { get; set; }

    public string EngName { get; set; } = null!;

    public string Name { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
