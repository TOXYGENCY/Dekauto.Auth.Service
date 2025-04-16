﻿using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Entities.DTO;
using Dekauto.Auth.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace Dekauto.Auth.Service.Services
{
    public class UserAuthService : IUserAuthService
    {
        private readonly IUsersRepository usersRepository;
        private readonly IRolesService rolesService;
        private readonly PasswordHasher<object> hasher; // object, а не User, потому что в этой реализации .HashPassword аргумент user не используется

        public UserAuthService(IUsersRepository usersRepository, IRolesService rolesService)
        {
            this.usersRepository = usersRepository;
            this.rolesService = rolesService;
            hasher = new PasswordHasher<object>();
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

            userDto.Password = null; // Формируемое Dto (для клиента) не содержит пароля, потому что в БД только хеши
            if (user.Role == null)
            {
                Console.WriteLine($"WARNING: роль == null у пользователя {user}");
                userDto.RoleName = null;
            } else
            {
                userDto.RoleName = user.Role.Name;
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

        public async Task UpdateUserAsync(Guid userId, UserDto updatedUserDto)
        {
            if (updatedUserDto == null || userId == null) throw new ArgumentNullException("Не все аргументы переданы.");
            if (updatedUserDto.Id != userId) throw new ArgumentException("ID не совпадают.");

            // Получаем текущего пользователя из репозитория
            var user = await usersRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"Пользователь с Id = {userId} не найден.");

            // Обновляем поля, которые должны измениться
            user.Login = updatedUserDto.Login;

            // Обновляем пароль, если он был передан (и если необходимо)
            if (!string.IsNullOrWhiteSpace(updatedUserDto.Password))
            {
                // Хешируем пароль перед сохранением
                user.PasswordHash = HashPassword(updatedUserDto.Password);
            }

            // Обновляем роль пользователя, если нужно (например, по имени роли)
            if (!string.IsNullOrWhiteSpace(updatedUserDto.RoleName))
            {
                var role = await rolesService.GetByRoleNameAsync(updatedUserDto.RoleName);
                if (role == null)
                    throw new InvalidOperationException($"Роль '{updatedUserDto.RoleName}' не найдена.");
                user.RoleId = role.Id;
                user.Role = role;
            }
            await usersRepository.UpdateAsync(user);
        }

        public async Task<bool> AuthenticateAsync(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password)) throw new ArgumentException();

            var account = await usersRepository.GetByLoginAsync(login);
            if (account == null) throw new KeyNotFoundException($"Пользователь {login} не найден");

            var result = hasher.VerifyHashedPassword(account, account.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task AddUserAsync(UserDto userDto)
        {
            if (userDto is null) throw new ArgumentNullException(nameof(userDto));

            var passwordHash = HashPassword(userDto.Password);
            var role = await rolesService.GetByRoleNameAsync(userDto.RoleName);
            if (role == null) throw new KeyNotFoundException($"Роль {userDto.RoleName} не найдена");

            var newUser = await FromDtoAsync(userDto);
            newUser.PasswordHash = passwordHash;
            newUser.RoleId = role.Id;

            await usersRepository.AddAsync(newUser);
        }
    }
}