namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IUserAuthService
    {
        Task<bool> AuthenticateAsync(string login, string password);
    }
}
