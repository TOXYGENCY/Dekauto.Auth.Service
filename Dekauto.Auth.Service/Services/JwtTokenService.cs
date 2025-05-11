using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Entities.DTO;
using Dekauto.Auth.Service.Domain.Entities.Models;
using Dekauto.Auth.Service.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Dekauto.Auth.Service.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration configuration;
        // Хранилище refresh токенов в памяти приложения - потокобезопасная реализация
        private static readonly ConcurrentDictionary<string, RefreshToken> refreshTokens =
            new ConcurrentDictionary<string, RefreshToken>();

        public JwtTokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // Публичный метод выдачи токенов
        public TokensModel GenerateTokens(UserDto account)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            var accessToken = GenerateAccessToken(account);
            var refreshToken = GenerateRefreshToken(account.Id);

            var tokensAdapter = new AccessTokenDto(accessToken, account);
            return new TokensModel(tokensAdapter, refreshToken);
        }

        // Метод генерации access токена
        private string GenerateAccessToken(UserDto account)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            // Создаем список "утверждений" (claims) - это данные, которые будут храниться в токене
            var claims = new List<Claim>
            {
                // Sub (Subject) - обычно имя пользователя или идентификатор
                new Claim(JwtRegisteredClaimNames.Sub, account.Login),
            
                // Jti (JWT ID) - уникальный идентификатор для каждого токена
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            
                // Кастомный claim с ID пользователя
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                
                // Добавляем роли пользователя
                new Claim(ClaimTypes.Role, account.EngRoleName)

            };
            try
            {
                // Создаем ключ для подписи токена из секретной строки в конфигурации
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));

                // Создаем учетные данные для подписи, используя алгоритм HMAC-SHA256
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Вычисляем время истечения токена
                var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["Jwt:ExpireMinutes"]));

                // Создаем сам JWT токен
                var token = new JwtSecurityToken(
                    issuer: configuration["Jwt:Issuer"],    // Кто выпустил токен
                    audience: configuration["Jwt:Audience"], // Для кого предназначен
                    claims: claims,                          // Наши утверждения (claims)
                    expires: expires,                        // Время истечения
                    signingCredentials: creds                // Ключ подписи
                );

                // Преобразуем токен в строку и возвращаем
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        // Метод генерации refresh токена
        private RefreshToken GenerateRefreshToken(Guid userId)
        {
            var newRefreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString("N"),
                UserId = userId,
                Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(configuration["Jwt:RefreshTokenExpireDays"] ?? "7"))
            };

            // Удаляем все существующие refresh-токены для этого пользователя
            var tokensToRemove = refreshTokens
                .Where(elem => elem.Value.UserId == userId)
                .Select(elem => elem.Key)
                .ToList();

            foreach (var tokenKey in tokensToRemove)
            {
                refreshTokens.TryRemove(tokenKey, out _);
            }

            refreshTokens.TryAdd(newRefreshToken.Token, newRefreshToken);
            return newRefreshToken;
        }

        public RefreshToken? GetRefreshToken(string refreshToken)
        {
            if (refreshToken is null)
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            if (!refreshTokens.TryGetValue(refreshToken, out var fullRefreshToken))
            {
                return null;
            }
            return fullRefreshToken;
        }

        // Метод проверки rt и выдачи новых
        public async Task<TryRefreshTokensModel> TryRefreshTokensAsync(string refreshToken, UserDto userDto)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException($"'{nameof(refreshToken)}' cannot be null or empty.", nameof(refreshToken));
            }

            if (userDto is null)
            {
                throw new ArgumentNullException(nameof(userDto));
            }

            TokensModel newTokens = null;

            // Получение rt из хранилища и проверка на просроченность
            if (!refreshTokens.TryRemove(refreshToken, out var fullToken)
                || fullToken.Expires < DateTime.UtcNow)
            {
                return new TryRefreshTokensModel(false, newTokens);
            }

            // Генерируем и выдаем новые токены, если все в порядке
            newTokens = GenerateTokens(userDto);
            return new TryRefreshTokensModel(true, newTokens);
        }

        // Внутренний метод проверки at
        public ClaimsPrincipal? ValidateAccessToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException($"'{nameof(token)}' cannot be null or empty.", nameof(token));
            }

            JwtSecurityTokenHandler tokenHandler;
            byte[] key;

            try
            {
                tokenHandler = new JwtSecurityTokenHandler();
                key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);
            }
            catch (Exception ex)
            {
                throw;
            }

            try
            {
                // Проверяем токен и получаем Principal (аутентифицированного пользователя)
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,     // Проверяем подпись
                    IssuerSigningKey = new SymmetricSecurityKey(key), // Ключ для проверки
                    ValidateIssuer = true,               // Проверяем издателя
                    ValidIssuer = configuration["Jwt:Issuer"], // Ожидаемый издатель
                    ValidateAudience = true,             // Проверяем получателя
                    ValidAudience = configuration["Jwt:Audience"], // Ожидаемый получатель
                    ValidateLifetime = true,             // Проверяем срок действия
                    ClockSkew = TimeSpan.Zero            // Не даем "льготного" времени
                }, out _);

                return principal;
            }
            catch
            {
                // Если что-то пошло не так (токен невалидный), возвращаем null
                return null;
            }
        }

        public ConcurrentDictionary<string, RefreshToken>? GetDict()
        {
            return refreshTokens;
        }
    }
}
