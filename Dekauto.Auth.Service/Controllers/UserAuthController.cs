using Dekauto.Auth.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dekauto.Auth.Service.Controllers
{
    [Route("api/auth/user")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly IUserAuthService userAuthService;

        public UserAuthController(IUserAuthService userAuthService)
        {
            this.userAuthService = userAuthService;
        }

        [HttpPost]
        public async Task<ActionResult> AuthenticateAndGetTokenAsync(string login, string password)
        {
            try
            {
                // TODO: Попытка авторизации и выдача JWT токена
                var token = await userAuthService.AuthenticateAndGetTokenAsync(login, password);
                if (token == null) throw new Exception("JWT-токен не получен.");
                // TODO: Управление refresh-токеном

                return Ok(token);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }
    }
}
