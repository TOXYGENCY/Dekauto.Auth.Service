using Dekauto.Auth.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Dekauto.Auth.Service.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration configuration;
        public JwtTokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // Метод генерации JWT токена
        public string GenerateToken(string login, Guid userId, string role)
        {
            // Создаем список "утверждений" (claims) - это данные, которые будут храниться в токене
            var claims = new List<Claim>
            {
                // Sub (Subject) - обычно имя пользователя или идентификатор
                new Claim(JwtRegisteredClaimNames.Sub, login),
            
                // Jti (JWT ID) - уникальный идентификатор для каждого токена
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            
                // Кастомный claim с ID пользователя
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                
                // Добавляем роли пользователя
                new Claim(ClaimTypes.Role, role)

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

        // Метод проверки токена
        public ClaimsPrincipal? ValidateToken(string token)
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
