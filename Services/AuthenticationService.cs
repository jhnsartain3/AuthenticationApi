using System;
using System.Threading.Tasks;
using Sartain_Studios_Common.Interfaces.Token;
using Sartain_Studios_Common.SharedModels;
using SharedModels;

namespace Services
{
    public interface IAuthenticationService
    {
        Task<TokenModel> GetAuthenticationTokenAsync(UserModel userModel);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IToken _token;
        private readonly IUserService _userService;

        public AuthenticationService(IUserService userService, IToken token)
        {
            _userService = userService;
            _token = token;
        }

        public async Task<TokenModel> GetAuthenticationTokenAsync(UserModel userModel)
        {
            await AnalyzeCredentialsForValidityAsync(userModel);

            var userId = await GetUserId(userModel);

            var userModelWithCompleteData = await GetUserInformation(userId);

            return GenerateToken(userModelWithCompleteData);
        }

        private async Task AnalyzeCredentialsForValidityAsync(UserModel userModel)
        {
            if (!await _userService.AreCredentialsValid(userModel))
                throw new UnauthorizedAccessException("Username or Password is invalid");
        }

        private async Task<string> GetUserId(UserModel userModel) => await _userService.GetUserId(userModel.Username);

        private async Task<UserModel> GetUserInformation(string userId) =>
            await _userService.GetUserInformation(userId);

        private TokenModel GenerateToken(UserModel userModelWithCompleteData) =>
            new() {Token = _token.GenerateToken(userModelWithCompleteData)};
    }
}