using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Entities.DTO;
using Dekauto.Auth.Service.Domain.Entities.Models;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IJwtTokenService
    {
        ConcurrentDictionary<string, RefreshToken>? GetDict();
        TokensModel GenerateTokens(UserDto account);
        RefreshToken? GetRefreshToken(string refreshToken);
        Task<TryRefreshTokensModel> TryRefreshTokensAsync(string refreshToken, UserDto userDto);
        ClaimsPrincipal? ValidateAccessToken(string token);
    }
}
