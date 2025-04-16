using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dekauto.Auth.Service.Infrastructure
{
    public class RolesRepository : IRolesRepository
    {
        private DekautoContext сontext;

        public RolesRepository(DekautoContext сontext)
        {
            this.сontext = сontext;
        }

        public async Task AddAsync(Role role)
        {
            сontext.Roles.Add(role);
            await сontext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            сontext.Remove(await GetByIdAsync(id));
            await сontext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await сontext.Roles.ToListAsync();
        }

        public async Task<Role> GetByIdAsync(Guid id)
        {
            return await сontext.Roles.FirstOrDefaultAsync(role => role.Id == id);
        }

        public async Task<Role> GetByRoleNameAsync(string name)
        {
            return await сontext.Roles.FirstOrDefaultAsync(role => role.Name == name);
        }

        public async Task UpdateAsync(Role updatedRole)
        {
            var currentRole = await сontext.Roles.FirstOrDefaultAsync(s => s.Id == updatedRole.Id);
            if (currentRole == null) throw new KeyNotFoundException($"Role {updatedRole.Id} not found");

            сontext.Entry(currentRole).CurrentValues.SetValues(updatedRole);
            await сontext.SaveChangesAsync();
        }
    }
}
