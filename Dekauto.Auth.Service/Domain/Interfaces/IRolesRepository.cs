using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Entities.DTO;

namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IRolesRepository : IRepository<Role>
    {
        Task<Role> GetByRoleNameAsync(string name);
    }
}
