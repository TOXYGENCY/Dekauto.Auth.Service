namespace Dekauto.Auth.Service.Domain.Entities.Adapters
{
    public class TryRefreshTokensResult
    {
        public bool Success { get; set; }
        public AuthTokensAdapter? Tokens { get; set; }

        public TryRefreshTokensResult(bool success, AuthTokensAdapter? tokens)
        {
            this.Success = success;
            this.Tokens = tokens;
        }
    }
}
