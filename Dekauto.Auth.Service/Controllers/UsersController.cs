using Dekauto.Auth.Service.Domain.Entities.DTO;
using Dekauto.Auth.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dekauto.Auth.Service.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserAuthService userAuthService;
        private readonly IUsersRepository usersRepository;

        public UsersController(IUserAuthService userAuthService, IUsersRepository usersRepository)
        {
            this.userAuthService = userAuthService;
            this.usersRepository = usersRepository;
        }

        // INFO: может вернуть пустой список
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await usersRepository.GetAllAsync();
                var usersDto = userAuthService.ToDtos(users);
                return Ok(usersDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserDto>> GetUserById(Guid userId)
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUserAsync(Guid userId, UserDto updatedUserDto)
        {
            try
            {
                await userAuthService.UpdateUserAsync(userId, updatedUserDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }


        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            try
            {
                if (userId == null) return StatusCode(StatusCodes.Status400BadRequest);
                await usersRepository.DeleteAsync(userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddUserAsync(UserDto userDto)
        {
            try
            {
                await userAuthService.AddUserAsync(userDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }
    }
}
