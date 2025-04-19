using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Entities.Adapters;
using Dekauto.Auth.Service.Domain.Entities.DTO;

namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IUserAuthService : IDtoConverter<User, UserDto>
    {
        Task<AuthTokensAdapter> AuthenticateAndGetTokensAsync(string login, string password);
        string HashPassword(string password);
        Task AddUserAsync(UserDto userDto, string password);
        Task UpdateUserAsync(Guid userId, UserDto updatedUserDto, string newPassword = null);

    }
}
