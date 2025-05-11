namespace Dekauto.Auth.Service.Domain.Entities.DTO
{
    public partial class UserDto
    {
        public Guid Id { get; set; }

        public string Login { get; set; }

        public string RoleName { get; set; }
        public string EngRoleName { get; set; }
    }
}
