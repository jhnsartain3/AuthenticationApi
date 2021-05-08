using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Sartain_Studios_Common.Interfaces.Token;
using Services;
using SharedModels;

namespace ServicesTests
{
    public class AuthenticationServiceTests
    {
        private IAuthenticationService _systemUnderTest;
        private Mock<IToken> _tokenMock;
        private Mock<IUserService> _userServiceMock;

        [SetUp]
        public void Setup()
        {
            _userServiceMock = new Mock<IUserService>();
            _tokenMock = new Mock<IToken>();

            _systemUnderTest = new AuthenticationService(_userServiceMock.Object, _tokenMock.Object);
        }

        [Test]
        public void GetAuthenticationTokenAsync_ThrowsException_If_CredentialsAreInvalid()
        {
            _userServiceMock.Setup(x => x.AreCredentialsValid(It.IsAny<UserModel>(), It.IsAny<string>()))
                .Returns(Task.FromResult(false));

            Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _systemUnderTest.GetAuthenticationTokenAsync(new UserModel()));
        }

        [Test]
        public async Task GetAuthenticationTokenAsync_Calls_GetUserId_With_UsernameAsync()
        {
            var sampleUserModel = new UserModel {Username = "SomeUsername"};

            _userServiceMock.Setup(x => x.AreCredentialsValid(It.IsAny<UserModel>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            var result = await _systemUnderTest.GetAuthenticationTokenAsync(sampleUserModel);

            _userServiceMock.Verify(x => x.GetUserId(It.Is<string>(x => x.Equals(sampleUserModel.Username)), null),
                Times.Once);
        }

        [Test]
        public async Task GetAuthenticationTokenAsync_Calls_GetUserInformation_With_UserIdAsync()
        {
            var sampleUserModel = new UserModel {Username = "SomeUsername", Password = "SomeUserPassword"};
            var userId = "SomeUserId";

            _userServiceMock.Setup(x => x.AreCredentialsValid(It.IsAny<UserModel>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            _userServiceMock.Setup(x => x.GetUserId(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(userId));

            var result = await _systemUnderTest.GetAuthenticationTokenAsync(sampleUserModel);

            _userServiceMock.Verify(x => x.GetUserInformation(It.Is<string>(x => x.Equals(userId)), null), Times.Once);
        }

        [Test]
        public async Task GetAuthenticationTokenAsync_Calls_GenerateToken_With_UserModelAsync()
        {
            var userId = "SomeUserId";
            var sampleUserModel = new UserModel {Id = userId, Username = "SomeUsername", Password = "SomeUserPassword"};

            _userServiceMock.Setup(x => x.AreCredentialsValid(It.IsAny<UserModel>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            _userServiceMock.Setup(x => x.GetUserId(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(userId));
            _userServiceMock.Setup(x => x.GetUserInformation(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(sampleUserModel));

            var result = await _systemUnderTest.GetAuthenticationTokenAsync(sampleUserModel);

            _tokenMock.Verify(x => x.GenerateToken(
                    It.Is<UserModel>(x =>
                        x.Username.Equals(sampleUserModel.Username)
                        && x.Password.Equals(sampleUserModel.Password)
                        && x.Id.Equals(sampleUserModel.Id))),
                Times.Once);
        }

        [Test]
        public async Task GetAuthenticationTokenAsync_Returns_ResultOfGetToken()
        {
            var userId = "SomeUserId";
            var sampleUserModel = new UserModel {Id = userId, Username = "SomeUsername", Password = "SomeUserPassword"};
            var someToken = "SomeToken";

            _userServiceMock.Setup(x => x.AreCredentialsValid(It.IsAny<UserModel>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            _userServiceMock.Setup(x => x.GetUserId(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(userId));
            _userServiceMock.Setup(x => x.GetUserInformation(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(sampleUserModel));
            _tokenMock.Setup(x => x.GenerateToken(It.IsAny<UserModel>())).Returns(someToken);

            var result = await _systemUnderTest.GetAuthenticationTokenAsync(sampleUserModel);

            Assert.AreEqual(someToken, result.Token);
        }
    }
}