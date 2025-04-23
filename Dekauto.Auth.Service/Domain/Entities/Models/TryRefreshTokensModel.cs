using Dekauto.Auth.Service.Domain.Entities.DTO;

namespace Dekauto.Auth.Service.Domain.Entities.Models
{
    public class TryRefreshTokensModel
    {
        public bool Success { get; set; }
        public TokensModel? Tokens { get; set; }

        public TryRefreshTokensModel(bool success, TokensModel? tokens)
        {
            Success = success;
            Tokens = tokens;
        }
    }
}
