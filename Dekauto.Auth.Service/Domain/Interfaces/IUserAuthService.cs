using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Entities.DTO;
using Dekauto.Auth.Service.Domain.Entities.Models;
using System.Collections.Concurrent;

namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IUserAuthService : IDtoConverter<User, UserDto>
    {
        Task<TokensModel> AuthenticateAndGetTokensAsync(string login, string password);
        string HashPassword(string password);
        Task AddUserAsync(UserDto userDto, string password);
        Task UpdateUserAsync(Guid userId, UserDto updatedUserDto, string newPassword = null);
        void SetRefreshTokenCookie(HttpResponse response, string refreshToken);
        RefreshToken? GetRefreshToken(string refreshTokenString);
        Task<TokensModel> RefreshTokensAsync(RefreshToken refreshToken);
        ConcurrentDictionary<string, RefreshToken>? GetDict();
    }
}
