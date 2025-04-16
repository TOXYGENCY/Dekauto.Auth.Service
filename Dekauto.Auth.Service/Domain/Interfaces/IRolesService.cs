using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Entities.DTO;

namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IRolesService : IDtoConverter<Role, RoleDto>
    {
        Task<Role> GetByRoleNameAsync(string name);
    }
}
