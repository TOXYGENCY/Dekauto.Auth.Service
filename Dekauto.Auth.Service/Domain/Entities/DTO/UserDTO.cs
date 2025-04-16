namespace Dekauto.Auth.Service.Domain.Entities.DTO
{
    public partial class UserDto
    {
        public Guid Id { get; set; }

        public string Login { get; set; }

        // Незахешированный пароль с клиента
        public string? Password { get; set; }

        public string RoleName { get; set; }
    }
}
