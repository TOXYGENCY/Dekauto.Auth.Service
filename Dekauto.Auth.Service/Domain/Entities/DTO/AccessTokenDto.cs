using System.IdentityModel.Tokens.Jwt;

namespace Dekauto.Auth.Service.Domain.Entities.DTO
{
    public class AccessTokenDto
    {
        public string AccessToken { get; }
        public DateTime AccessTokenExpiry { get; }
        public UserDto User { get; }

        public AccessTokenDto(string accessToken, UserDto userDto)
        {
            AccessToken = accessToken;
            User = userDto;

            // вычисление даты истечения access token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            AccessTokenExpiry = jwtToken.ValidTo;
        }
    }
}
