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
        private readonly ILogger<UserAuthController> logger;

        public UserAuthController(IUserAuthService userAuthService, ILogger<UserAuthController> logger)
        {
            this.userAuthService = userAuthService;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> AuthenticateAndGetTokensAsync([FromBody] LoginAdapter loginUser)
        {
            try
            {
                if (loginUser is null) throw new ArgumentNullException(nameof(loginUser));

                // Попытка авторизации и выдача JWT access токена и refresh токена
                var tokensModel = await userAuthService.AuthenticateAndGetTokensAsync(loginUser.Login, loginUser.Password);
                if (tokensModel is null) return StatusCode(StatusCodes.Status401Unauthorized);

                // Устанавливаем rt в HttpOnly cookie
                userAuthService.SetRefreshTokenCookie(Response, tokensModel.RefreshToken.Token);

                return Ok(tokensModel.TokenAdapter);
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex, $"User {loginUser.Login} was not found during authentication attempt.");
                return StatusCode(StatusCodes.Status404NotFound,
                    $"Пользователь {loginUser.Login} не найден при попытке аутентификации.");
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "No account login data was received.");
                // Но может быть и InternalServerError
                return StatusCode(StatusCodes.Status400BadRequest,
                    "Не получены данные входа в аккаунт. Обратитесь к администратору или попробуйте снова.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected server error has occurred.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpPost("{userId}/refresh")]
        public async Task<ActionResult> RefreshTokensAsync(Guid userId)
        {
            try
            {
                // Получаем refresh token ИЗ КУКИ (не из тела запроса)
                var refreshTokenString = Request.Cookies["refreshToken"];
                if (String.IsNullOrEmpty(refreshTokenString))
                {
                    logger.LogError(null, "Refresh token is missing");
                    return StatusCode(StatusCodes.Status400BadRequest,
                        "Пожалуйста, войдите снова. (Refresh-токен отсутствует)");
                }
                var fullRefreshToken = userAuthService.GetRefreshToken(refreshTokenString);
                if (fullRefreshToken is null)
                {
                    logger.LogError(null, "Refresh token not found in the registry");
                    // Не 401, потому что фронт будет циклиться на 401 и никуда не продвинется
                    return StatusCode(StatusCodes.Status403Forbidden,
                        "Пожалуйста, войдите снова. (Refresh-токен не найден в реестре)");
                }

                var newTokens = await userAuthService.RefreshTokensAsync(fullRefreshToken);
                if (newTokens is null)
                {
                    logger.LogError(null, "Refresh token is expired");
                    // Не 401, потому что фронт будет циклиться на 401 и никуда не продвинется
                    return StatusCode(StatusCodes.Status403Forbidden,
                        "Пожалуйста, войдите снова. (Refresh-токен просрочен)");
                }

                // Устанавливаем rt в HttpOnly cookie
                userAuthService.SetRefreshTokenCookie(Response, newTokens.RefreshToken.Token);

                return Ok(newTokens.TokenAdapter);
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "The required data is not provided.");
                return StatusCode(StatusCodes.Status400BadRequest,
                    "Не переданы необходимые данные. Обратитесь к администратору или попробуйте позже");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected server error has occurred.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.");
            }
        }

        // Метод, который будет кидать ответ 401, если токен невалиден (проверка по параметрам из Program.cs)
        [HttpGet("validate")]
        [Authorize] // Здесь происходит проверка и будет вылетать ответ 401 Unauthorized
        public IActionResult ValidateAccessToken()
        {
            // Доступ к данным пользователя User (из Authorize) из токена
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var login = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new { UserId = userId, Login = login, Role = role });
        }
    }
}
