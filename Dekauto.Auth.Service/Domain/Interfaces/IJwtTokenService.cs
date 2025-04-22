using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Entities.Adapters;
using Dekauto.Auth.Service.Domain.Entities.DTO;
using System.Security.Claims;

namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IJwtTokenService
    {
        AuthTokensAdapter GenerateTokens(UserDto account);
        //string GenerateAccessToken(UserDto account);
        //RefreshToken GenerateRefreshToken(Guid userId);
        Task<TryRefreshTokensResult> TryRefreshTokensAsync(string refreshToken, UserDto userDto);
        ClaimsPrincipal? ValidateAccessToken(string token);
    }
}
