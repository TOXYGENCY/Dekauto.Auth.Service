namespace Dekauto.Auth.Service.Domain.Entities
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }
    }
}
