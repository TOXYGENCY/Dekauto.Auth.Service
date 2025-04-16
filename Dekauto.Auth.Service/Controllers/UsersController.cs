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

        [HttpPost]
        public async Task<ActionResult> AddUser(UserDto userDto)
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
