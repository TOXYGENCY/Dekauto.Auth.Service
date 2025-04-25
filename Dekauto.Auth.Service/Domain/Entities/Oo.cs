using System.Text.Json.Serialization;

namespace Dekauto.Auth.Service.Domain.Entities;

public partial class Oo
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? OoAddress { get; set; }

    [JsonIgnore]

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
