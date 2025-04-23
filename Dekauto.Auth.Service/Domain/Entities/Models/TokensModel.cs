using Dekauto.Auth.Service.Domain.Entities.DTO;

namespace Dekauto.Auth.Service.Domain.Entities.Models
{
    public class TokensModel
    {
        public AccessTokenDto TokenAdapter;
        public RefreshToken RefreshToken;

        public TokensModel(AccessTokenDto tokenAdapter, RefreshToken refreshToken)
        {
            TokenAdapter = tokenAdapter;
            RefreshToken = refreshToken;
        }
    }
}