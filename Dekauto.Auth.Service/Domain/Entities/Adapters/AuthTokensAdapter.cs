using Dekauto.Auth.Service.Domain.Entities.DTO;

namespace Dekauto.Auth.Service.Domain.Entities.Adapters
{
    public class AuthTokensAdapter
    {
        public string AccessToken { get; set; }
        public UserDto UserDtoData { get; set; }

        //public string RefreshToken { get; set; }

        public AuthTokensAdapter(string accessToken, UserDto userDtoData)
        {
            AccessToken = accessToken;
            UserDtoData = userDtoData;
        }

    }
}
