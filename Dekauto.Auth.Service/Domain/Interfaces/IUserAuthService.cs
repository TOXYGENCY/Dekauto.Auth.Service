using Dekauto.Auth.Service.Domain.Entities.DTO;

namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IUserAuthService
    {
        Task<bool> AuthenticateAsync(string login, string password);
        string HashPassword(string password);
        Task AddUserAsync(UserDto userDto);
    }
}
