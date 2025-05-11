using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Entities.DTO;
using Dekauto.Auth.Service.Domain.Entities.Models;
using Dekauto.Auth.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Dekauto.Auth.Service.Services
{
    public class UserAuthService : IUserAuthService
    {
        private readonly IUsersRepository usersRepository;
        private readonly IRolesService rolesService;
        private readonly IJwtTokenService jwtTokenService;
        private readonly IConfiguration configuration;
        private readonly PasswordHasher<object> hasher; // object, а не User, потому что в этой реализации .HashPassword аргумент user не используется

        public UserAuthService(IUsersRepository usersRepository, IRolesService rolesService,
            IJwtTokenService jwtTokenService, IConfiguration configuration)
        {
            this.usersRepository = usersRepository;
            this.jwtTokenService = jwtTokenService;
            this.configuration = configuration;
            this.rolesService = rolesService;
            hasher = new PasswordHasher<object>();
        }

        public async Task<TokensModel> AuthenticateAndGetTokensAsync(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password)) throw new ArgumentException();

            var userAccount = await usersRepository.GetByLoginAsync(login);
            if (userAccount == null) throw new KeyNotFoundException($"Пользователь {login} не найден");

            var result = hasher.VerifyHashedPassword(userAccount, userAccount.PasswordHash, password);
            // Проверка данных и выдача at + rt + данных пользователя, если успех
            if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                var tokensModel = jwtTokenService.GenerateTokens(ToDto(userAccount));
                return tokensModel;
            }
            else
            {
                return null;
            }
        }


        // Передача refresh токена только через HttpOnly куки, недоступный для JS. (вызывать в контроллере)
        public void SetRefreshTokenCookie(HttpResponse response, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(configuration["Jwt:RefreshTokenExpireDays"] ?? "7")),
                Secure = Boolean.Parse(configuration["UseHttps"]), // HTTPS
                SameSite = SameSiteMode.Strict, // Защита от CSRF
                Path = "/api/auth"
            };

            // Отдельные настройки для удаления
            var deleteOptions = new CookieOptions
            {
                Path = "/", // Широкий путь для гарантированного удаления
                Secure = true, // Обеспечиваем удаление и для HTTP, и для HTTPS
                SameSite = SameSiteMode.None // Для максимальной совместимости при удалении
            };

            if (response is null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException($"'{nameof(refreshToken)}' cannot be null or empty.", nameof(refreshToken));
            }

            // Удаляем старую куку перед установкой новой
            response.Cookies.Delete("refreshToken", deleteOptions);
            // Ставим новую куку
            response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        // хеширование пароля
        public string HashPassword(string password)
        {
            return hasher.HashPassword(null, password); // null, а не User, потому что в этой реализации .HashPassword аргумент user не используется
        }

        /// <summary>
        /// Конвертирование из объекта src типа SRC в объект типа DEST через сериализацию и десереализацию в JSON-объект (встроенный авто-маппинг).
        /// </summary>
        /// <typeparam name="SRC"></typeparam>
        /// <typeparam name="DEST"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public DEST JsonSerializationConvert<SRC, DEST>(SRC src)
        {
            return JsonSerializer.Deserialize<DEST>(JsonSerializer.Serialize(src));
        }

        public async Task<User> FromDtoAsync(UserDto userDto)
        {
            if (userDto == null) throw new ArgumentNullException(nameof(userDto));

            var user = await usersRepository.GetByIdAsync(userDto.Id);
            user ??= JsonSerializationConvert<UserDto, User>(userDto);

            return user;
        }

        public UserDto ToDto(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var userDto = JsonSerializationConvert<User, UserDto>(user);

            if (user.Role == null)
            {
                Console.WriteLine($"WARNING: роль == null у пользователя {user}");
                userDto.RoleName = null;
                userDto.EngRoleName = null;
            }
            else
            {
                userDto.RoleName = user.Role.Name;
                userDto.EngRoleName = user.Role.EngName;
            }
            return userDto;
        }

        public IEnumerable<UserDto> ToDtos(IEnumerable<User> users)
        {
            if (users == null) throw new ArgumentNullException(nameof(users));

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                userDtos.Add(ToDto(user));
            }

            return userDtos;
        }

        public async Task UpdateUserAsync(Guid userId, UserDto updatedUserDto, string newPassword = null)
        {
            if (updatedUserDto == null || userId == null) throw new ArgumentNullException("Не все аргументы переданы.");
            if (updatedUserDto.Id != userId) throw new ArgumentException("ID не совпадают.");

            // Получаем текущего пользователя из репозитория
            var user = await usersRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"Пользоват  ель с Id = {userId} не найден.");

            // Обновляем поля, которые должны измениться
            user.Login = updatedUserDto.Login;

            // Обновляем пароль, если он был передан
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                // Хешируем пароль перед сохранением
                user.PasswordHash = HashPassword(newPassword);
            }

            // Обновляем роль пользователя, если нужно (например, по имени роли)
            if (!string.IsNullOrWhiteSpace(updatedUserDto.RoleName))
            {
                var role = await rolesService.GetByRoleNameAsync(updatedUserDto.RoleName);
                if (role == null)
                    throw new InvalidOperationException($"Роль '{updatedUserDto.EngRoleName}' не найдена.");
                user.RoleId = role.Id;
                user.Role = role;
            }
            await usersRepository.UpdateAsync(user);
        }

        public async Task AddUserAsync(UserDto userDto, string password)
        {
            if (userDto is null) throw new ArgumentNullException(nameof(userDto));

            var passwordHash = HashPassword(password);
            var role = await rolesService.GetByRoleNameAsync(userDto.RoleName);
            if (role == null) throw new KeyNotFoundException($"Роль {userDto.EngRoleName} не найдена");

            var newUser = await FromDtoAsync(userDto);
            newUser.PasswordHash = passwordHash;
            newUser.RoleId = role.Id;

            await usersRepository.AddAsync(newUser);
        }

        public async Task<TokensModel> RefreshTokensAsync(RefreshToken refreshToken)
        {
            if (refreshToken is null)
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            var user = await usersRepository.GetByIdAsync(refreshToken.UserId);
            var result = await jwtTokenService.TryRefreshTokensAsync(refreshToken.Token, ToDto(user));
            TokensModel? newTokens;

            if (result.Success)
            {
                newTokens = result.Tokens;
            }
            else
            {
                newTokens = null;
            }

            return newTokens;
        }

        public RefreshToken? GetRefreshToken(string refreshTokenString)
        {
            return jwtTokenService.GetRefreshToken(refreshTokenString);
        }

        public ConcurrentDictionary<string, RefreshToken>? GetDict()
        {
            return jwtTokenService.GetDict();
        }
    }
}