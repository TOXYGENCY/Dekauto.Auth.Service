using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dekauto.Auth.Service.Controllers
{
    [Route("api/auth/user")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> AuthenticateUser(string login, string password)
        {
            throw new NotImplementedException();
        }
    }
}
