using Dekauto.Auth.Service.Domain.Entities;
using Dekauto.Auth.Service.Domain.Entities.Adapters;
using Dekauto.Auth.Service.Domain.Entities.DTO;
using Dekauto.Auth.Service.Domain.Entities.Models;
using Dekauto.Auth.Service.Domain.Interfaces;
using Dekauto.Auth.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql.TypeMapping;
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

		// TODO: удалить
		[HttpGet]
		public async Task<ActionResult> DebugDict()
		{
			try
			{
				return Ok(userAuthService.GetDict());
			}
			catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }

            [HttpPost]
		public async Task<ActionResult> AuthenticateAndGetTokensAsync([FromBody] LoginAdapter loginUser)
		{
			try
			{
				if (loginUser is null) throw new ArgumentNullException(nameof(loginUser));
				// TODO: Попытка авторизации и выдача JWT access токена и refresh токена
				var tokensModel = await userAuthService.AuthenticateAndGetTokensAsync(loginUser.Login, loginUser.Password);
				//if (tokensModel == null) throw new Exception("Токены доступа не получены.");
				if (tokensModel is null) return StatusCode(StatusCodes.Status401Unauthorized);

				// Устанавливаем rt в HttpOnly cookie
				userAuthService.SetRefreshTokenCookie(Response, tokensModel.RefreshToken.Token);

				return Ok(tokensModel.TokenAdapter);
			}
			catch (KeyNotFoundException ex)
			{
				return StatusCode(StatusCodes.Status404NotFound, new { ex.Message, ex.StackTrace });
			}
			catch (ArgumentNullException ex) 
			{
				// Но может быть и InternalServerError
				return StatusCode(StatusCodes.Status400BadRequest, new { ex.Message, ex.StackTrace });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
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
                    return StatusCode(StatusCodes.Status400BadRequest, "Refresh token is missing");
                }
				var fullRefreshToken = userAuthService.GetRefreshToken(refreshTokenString);
                if (fullRefreshToken is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, 
						"Refresh token is not found, as it might have expired. Please log in again.");

                var newTokens = await userAuthService.RefreshTokensAsync(fullRefreshToken);
                if (newTokens is null)
                    return StatusCode(StatusCodes.Status401Unauthorized,
                        "Refresh token is expired. Please log in again.");

                // Устанавливаем rt в HttpOnly cookie
                userAuthService.SetRefreshTokenCookie(Response, newTokens.RefreshToken.Token);

                return Ok(newTokens.TokenAdapter);
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

        // Метод, который будет кидать ошибку 401, если токен невалиден (проверка по параметрам из Program.cs)
        [HttpGet("validate")]
		[Authorize] // Здесь происходит проверка и будет вылетать 401 Unauthorized
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
