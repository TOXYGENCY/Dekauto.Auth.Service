using Dekauto.Auth.Service.Domain.Entities;

namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IRolesRepository : IRepository<Role>
    {
        Task<Role> GetByRoleNameAsync(string name);
    }
}
