using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Entities.Adapters;
using Dekauto.Auth.Service.Domain.Entities.DTO;
using Dekauto.Auth.Service.Domain.Interfaces;
using Dekauto.Auth.Service.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
        public AuthTokensAdapter GenerateTokens(UserDto account)
        {
            var accessToken = GenerateAccessToken(account);
            var refreshToken = GenerateRefreshToken(account.Id);

            return new AuthTokensAdapter(accessToken, refreshToken.Token, account);
        }

        // Метод генерации access токена
        private string GenerateAccessToken(UserDto account)
        {

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
                new Claim(ClaimTypes.Role, account.RoleName)

            };
            try
            {
                // Создаем ключ для подписи токена из секретной строки в конфигурации
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));

                // Создаем учетные данные для подписи, используя алгоритм HMAC-SHA256
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Вычисляем время истечения токена
                var expires = DateTime.Now.AddMinutes(Convert.ToDouble(configuration["Jwt:ExpireMinutes"]));

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
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString("N"),
                UserId = userId,
                Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(configuration["Jwt:RefreshTokenExpireDays"] ?? "7")),
                Created = DateTime.UtcNow
            };

            refreshTokens.AddOrUpdate(refreshToken.Token, refreshToken, (k, v) => refreshToken);
            return refreshToken;
        }

        // Метод проверки rt и выдачи новых
        public async Task<TryRefreshTokensResult> TryRefreshTokensAsync(string refreshToken, UserDto userDto)
        {
            AuthTokensAdapter newTokens = null;

            // Получение rt из хранилища и проверка на просроченность
            if (!refreshTokens.TryGetValue(refreshToken, out var storedToken) 
                || storedToken.Expires <= DateTime.UtcNow)
            {
                return new TryRefreshTokensResult(false, newTokens);
            }

            // Удаляем использованный refresh token
            refreshTokens.TryRemove(refreshToken, out _);

            // Генерируем и выдаем новые токены, если все в порядке
            newTokens = GenerateTokens(userDto);
            return new TryRefreshTokensResult(true, newTokens);
        }

        // Внутренний метод проверки at
        public ClaimsPrincipal? ValidateAccessToken(string token)
        {
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
    }
}
