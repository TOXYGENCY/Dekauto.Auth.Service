using Dekauto.Auth.Service.Domain.Entities.DTO;
using System.IdentityModel.Tokens.Jwt;

namespace Dekauto.Auth.Service.Domain.Entities.Adapters
{
    public class AuthTokensAdapter
    {
        public string AccessToken { get; }
        public string RefreshToken { get; }
        public UserDto User { get; }
        public DateTime AccessTokenExpiry { get; }

        public AuthTokensAdapter(string accessToken, string refreshToken, UserDto userDto)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            User = userDto;

            // вычисление даты истечения access token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            AccessTokenExpiry = jwtToken.ValidTo;
        }
    }
}
