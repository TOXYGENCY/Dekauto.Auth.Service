using Dekauto.Auth.Service.Domain.Entities.DTO;
using Dekauto.Auth.Service.Domain.Interfaces;
using Dekauto.Auth.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;

namespace Dekauto.Auth.Service.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Policy = "OnlyAdmin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserAuthService userAuthService;
        private readonly IUsersRepository usersRepository;
        private readonly ILogger<UserAuthController> logger;

        public UsersController(IUserAuthService userAuthService, IUsersRepository usersRepository,
            ILogger<UserAuthController> logger)
        {
            this.userAuthService = userAuthService;
            this.usersRepository = usersRepository;
            this.logger = logger;
        }

        // INFO: может вернуть пустой список
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await usersRepository.GetAllAsync();
                var usersDto = userAuthService.ToDtos(users);
                return Ok(usersDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while retrieving all users.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при получении всех пользователей. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserDto>> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await usersRepository.GetByIdAsync(userId);
                if (user == null) return StatusCode(StatusCodes.Status404NotFound);
                var userDto = userAuthService.ToDto(user);

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while searching for the user");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при поиске пользователя. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpPost("{userId}/changepass")]
        public async Task<IActionResult> UpdateUserPasswordAsync(Guid userId, string newPassword, string currentPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(newPassword))
                {
                    throw new ArgumentException($"'{nameof(newPassword)}' cannot be null or empty.", nameof(newPassword));
                }

                if (string.IsNullOrEmpty(currentPassword))
                {
                    throw new ArgumentException($"'{nameof(currentPassword)}' cannot be null or empty.", nameof(currentPassword));
                }

                await userAuthService.ChangePasswordAsync(userId, newPassword, currentPassword);

                return Ok();

            }
            catch (InvalidCredentialException ex)
            {
                logger.LogError(ex, "Invalid password.");
                return StatusCode(StatusCodes.Status403Forbidden, "Указан неверный пароль.");
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Not enough arguments passed to change the password");
                return StatusCode(StatusCodes.Status400BadRequest,
                    "Возникла непредвиденная ошибка при изменении пароля. Обратитесь к администратору или попробуйте позже.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while changing the password");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при изменении пароля. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUserAsync(Guid userId, UserDto updatedUserDto, 
                                                            string? newPassword = null)
        {
            try
            {
                await userAuthService.UpdateUserAsync(userId, updatedUserDto, newPassword);
                logger.LogInformation($"User {updatedUserDto.Login} updated (Role: {updatedUserDto.EngRoleName})");

                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogError(ex, "User not found");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Пользователь не найден.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while updating the user");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при обновлении пользователя. Обратитесь к администратору или попробуйте позже.");
            }
        }


        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUserAsync(Guid userId)
        {
            try
            {
                await usersRepository.DeleteAsync(userId);
                logger.LogInformation($"User with id = {userId} has been deleted");

                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while deleting the user.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при удалении пользователя. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddUserAsync(UserDto userDto, string password)
        {
            try
            {
                if (userDto is null)
                {
                    throw new ArgumentNullException(nameof(userDto));
                }

                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentNullException($"'{nameof(password)}' cannot be null or empty.", nameof(password));
                }

                await userAuthService.AddUserAsync(userDto, password);
                logger.LogInformation($"User {userDto.Login} created (Role: {userDto.EngRoleName})");

                return Ok();
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "Required data to add a user is not received.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Необходимые данные для добавления пользователя не получены.Обратитесь к администратору или попробуйте позже.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while adding the user.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при добавлении пользователя. Обратитесь к администратору или попробуйте позже.");
            }
        }
    }
}
