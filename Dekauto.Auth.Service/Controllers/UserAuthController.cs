using Dekauto.Auth.Service.Domain.Entities.Adapters;
using Dekauto.Auth.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dekauto.Auth.Service.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly IUserAuthService userAuthService;

        public UserAuthController(IUserAuthService userAuthService)
        {
            this.userAuthService = userAuthService;
        }

        [HttpPost]
        public async Task<ActionResult> AuthenticateAndGetTokensAsync([FromBody] LoginAdapter loginUser)
        {

            try
            {
                if (loginUser is null) throw new ArgumentNullException(nameof(loginUser));
                // TODO: Попытка авторизации и выдача JWT access токена и refresh токена
                var tokensAdapter = await userAuthService.AuthenticateAndGetTokensAsync(loginUser.Login, loginUser.Password);
                //if (tokensAdapter == null) throw new Exception("Токены доступа не получены.");

                return Ok(tokensAdapter);
            }
            catch (KeyNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { ex.Message, ex.StackTrace });
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { ex.Message, ex.StackTrace });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }

        // Метод, который будет кидать ошибку 401, если токен невалиден - фронт разлогинит пользователя
        [HttpGet("validate")]
        [Authorize] // Отсюда будет вылетать 401 Unauthorized
        public IActionResult Validate()
        {
            // Доступ к данным пользователя User (из Authorize) из токена
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var login = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new { UserId = userId, Login = login, Role = role });
        }
    }
}
