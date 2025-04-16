using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dekauto.Auth.Service.Infrastructure
{
    public class UsersRepository : IUsersRepository
    {
        private DekautoContext сontext;

        public UsersRepository(DekautoContext сontext)
        {
            this.сontext = сontext;
        }

        public async Task AddAsync(User user)
        {
            сontext.Users.Add(user);
            await сontext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            сontext.Remove(await GetByIdAsync(id));
            await сontext.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await сontext.Users.ToListAsync();
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await сontext.Users.FirstOrDefaultAsync(user => user.Id == id);
        }

        public async Task<User> GetByLoginAsync(string login)
        {
            return await сontext.Users.FirstOrDefaultAsync(user => user.Login == login);
        }

        public async Task UpdateAsync(User updatedUser)
        {
            var currentUser = await сontext.Users.FirstOrDefaultAsync(s => s.Id == updatedUser.Id);
            if (currentUser == null) throw new KeyNotFoundException($"User {updatedUser.Id} not found");

            сontext.Entry(currentUser).CurrentValues.SetValues(updatedUser);
            await сontext.SaveChangesAsync();
        }
    }
}
