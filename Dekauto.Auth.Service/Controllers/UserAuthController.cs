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
        public async Task<ActionResult> AuthenticateUser(string login, string password)
        {
            try
            {
                // TODO: Попытка авторизации
                var result = await userAuthService.AuthenticateAsync(login, password);
                // TODO: Выдача JWT токена

                // TODO: Управление refresh-токеном

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }
    }
}
