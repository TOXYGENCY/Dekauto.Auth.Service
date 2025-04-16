using Dekauto.Auth.Service.Domain.Entities;

namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IUsersRepository : IRepository<User>
    {
        Task<User> GetByLoginAsync(string login);
    }
}
