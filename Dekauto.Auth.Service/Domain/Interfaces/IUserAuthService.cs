using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Entities.DTO;

namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IUserAuthService : IDtoConverter<User, UserDto>
    {
        Task<string> AuthenticateAndGetTokenAsync(string login, string password);
        string HashPassword(string password);
        Task AddUserAsync(UserDto userDto);
        Task UpdateUserAsync(Guid userId, UserDto updatedUserDto);

    }
}
