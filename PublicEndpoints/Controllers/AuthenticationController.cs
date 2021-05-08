using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PublicEndpoints.Controllers.Base;
using Sartain_Studios_Common.SharedModels;
using Services;
using SharedModels;

namespace PublicEndpoints.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : BaseController
    {
        private readonly IAuthenticationService _authentication;

        public AuthenticationController(IAuthenticationService authentication) => _authentication = authentication;

        [HttpPost("Token")]
        public async Task<ActionResult<TokenModel>> GetAuthenticationToken(UserModel userModel)
        {
            LoggerWrapper.LogInformation("GetAuthenticationToken", GetType().Name, nameof(GetAuthenticationToken),
                null);
            return await _authentication.GetAuthenticationTokenAsync(userModel);
        }
    }
}