namespace Dekauto.Auth.Service.Domain.Entities.DTO
{
    public partial class UserDTO
    {
        public Guid Id { get; set; }

        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string RoleName { get; set; } = null!;
    }
}
