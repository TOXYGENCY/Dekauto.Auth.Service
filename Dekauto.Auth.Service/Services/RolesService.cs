using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Entities.DTO;
using Dekauto.Auth.Service.Domain.Interfaces;
using System.Text.Json;

namespace Dekauto.Auth.Service.Services
{
    public class RolesService : IRolesService
    {
        private readonly IRolesRepository rolesRepository;

        public RolesService(IRolesRepository rolesRepository)
        {
            this.rolesRepository = rolesRepository;
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

        public async Task<Role> FromDtoAsync(RoleDto roleDto)
        {
            if (roleDto == null) throw new ArgumentNullException(nameof(roleDto));

            var role = await rolesRepository.GetByRoleNameAsync(roleDto.EngName);
            if (role == null) throw new KeyNotFoundException($"Роль {roleDto.EngName} не найдена");

            return role;
        }

        public RoleDto ToDto(Role role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            return JsonSerializationConvert<Role, RoleDto>(role);
        }

        public IEnumerable<RoleDto> ToDtos(IEnumerable<Role> roles)
        {
            if (roles == null) throw new ArgumentNullException(nameof(roles));

            var roleDtos = new List<RoleDto>();
            foreach (var role in roles)
            {
                roleDtos.Add(JsonSerializationConvert<Role, RoleDto>(role));
            }

            return roleDtos;
        }

        public async Task<Role> GetByRoleNameAsync(string name)
        {
            return await rolesRepository.GetByRoleNameAsync(name);
        }
    }
}
