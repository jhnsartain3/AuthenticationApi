using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Sartain_Studios_Common.HttpRestApiCalls;
using Sartain_Studios_Common.Interfaces.Token;
using Sartain_Studios_Common.SharedEntities;
using Sartain_Studios_Common.SharedModels;
using Services;
using SharedModels;

namespace ServicesTests
{
    public class UserServiceTests
    {
        private Mock<IAutoWrapperHttpClient<UserModel>> _httpClientWrapperMock;
        private IUserService _systemUnderTest;
        private Mock<IToken> _tokenMock;

        [SetUp]
        public void Setup()
        {
            _httpClientWrapperMock = new Mock<IAutoWrapperHttpClient<UserModel>>();
            _tokenMock = new Mock<IToken>();

            _systemUnderTest = new UserService(_httpClientWrapperMock.Object, _tokenMock.Object);
        }

        [Test]
        public async Task AreCredentialsValid_Calls_PostWithResultAsync_WithExpectedParametersAsync()
        {
            var userModel = new UserModel {Username = "SomeUsername", Password = "SomePassword"};
            const string token = "someToken";

            _tokenMock.Setup(x =>
                x.GenerateToken(It.Is<UserModel>(x => x.Roles[0].Equals(Role.Service)))).Returns("SomeOtherToken");
            _httpClientWrapperMock.Setup(x =>
                    x.PostWithResultAsync("User/Credentials/Valid",
                        It.Is<UserModel>(
                            x => x.Username.Equals(userModel.Username) && x.Password.Equals(userModel.Password)),
                        token))
                .Returns(Task.FromResult(new AutoWrapperResponseModel<string> {Result = "true"}));

            await _systemUnderTest.AreCredentialsValid(userModel, token);

            _httpClientWrapperMock.Verify(x =>
                x.PostWithResultAsync("User/Credentials/Valid", userModel, token), Times.Once);
        }

        [Test]
        public async Task AreCredentialsValid_Calls_PostWithResultAsync_WithBaseClassTokenForParameterAsync()
        {
            var userModel = new UserModel {Username = "SomeUsername", Password = "SomePassword"};

            _tokenMock.Setup(x => x.GenerateToken(
                    It.Is<UserModel>(x => x.Roles[0].Equals(Role.Service))))
                .Returns("SomeOtherToken");
            _httpClientWrapperMock.Setup(x =>
                    x.PostWithResultAsync("User/Credentials/Valid", It.Is<UserModel>(x =>
                            x.Username.Equals(userModel.Username)
                            && x.Password.Equals(userModel.Password)),
                        "SomeOtherToken"))
                .Returns(Task.FromResult(new AutoWrapperResponseModel<string>
                    {Result = "true"}));

            await _systemUnderTest.AreCredentialsValid(userModel);

            _tokenMock.Verify(x => x.GenerateToken(
                It.Is<UserModel>(x => x.Roles[0].Equals(Role.Service))), Times.Once);
            _httpClientWrapperMock.Verify(
                x => x.PostWithResultAsync("User/Credentials/Valid", userModel, "SomeOtherToken"), Times.Once);
        }

        [Test]
        public async Task AreCredentialsValid_ReturnsFalse_If_PostWithResultAsync_ReturnsFalse()
        {
            var userModel = new UserModel {Username = "SomeUsername", Password = "SomePassword"};
            const string token = "someToken";

            _tokenMock.Setup(x => x.GenerateToken(It.Is<UserModel>(x => x.Roles[0].Equals(Role.Service))))
                .Returns("SomeOtherToken");
            _httpClientWrapperMock
                .Setup(x => x.PostWithResultAsync("User/Credentials/Valid", It.IsAny<UserModel>(), token))
                .Returns(Task.FromResult(new AutoWrapperResponseModel<string> {Result = "false"}));

            var result = await _systemUnderTest.AreCredentialsValid(userModel, token);

            Assert.AreEqual(false, result);
        }

        [Test]
        public async Task AreCredentialsValid_ReturnsTrue_If_PostWithResultAsync_ReturnsTrue()
        {
            var userModel = new UserModel {Username = "SomeUsername", Password = "SomePassword"};
            const string token = "someToken";

            _tokenMock.Setup(x => x.GenerateToken(It.Is<UserModel>(x =>
                    x.Roles[0].Equals(Role.Service))))
                .Returns("SomeOtherToken");
            _httpClientWrapperMock
                .Setup(x =>
                    x.PostWithResultAsync("User/Credentials/Valid", It.IsAny<UserModel>(), token))
                .Returns(Task.FromResult(new AutoWrapperResponseModel<string>
                    {Result = "true"}));

            var result = await _systemUnderTest.AreCredentialsValid(userModel, token);

            Assert.AreEqual(true, result);
        }

        [Test]
        public async Task GetUserId_Calls_PostWithResultAsync_WithExpectedParametersAsync()
        {
            const string username = "someUsername";
            const string token = "someToken";
            const string userId = "someUserId";

            _tokenMock.Setup(x => x.GenerateToken(It.Is<UserModel>(x =>
                    x.Roles[0].Equals(Role.Service))))
                .Returns("SomeOtherToken");
            _httpClientWrapperMock
                .Setup(x => x.GetByIdAsync<string>("User/Username",
                    It.Is<string>(x => x.Equals(username)), token))
                .Returns(Task.FromResult(new AutoWrapperResponseModel<string>
                    {Result = userId}));

            await _systemUnderTest.GetUserId(username, token);

            _httpClientWrapperMock.Verify(x =>
                x.GetByIdAsync<string>("User/Username", username, token), Times.Once);
        }

        [Test]
        public async Task GetUserId_Calls_PostWithResultAsync_WithBaseClassTokenForParameterAsync()
        {
            const string username = "someUsername";
            const string token = "someToken";
            const string userId = "someUserId";

            _tokenMock.Setup(x => x.GenerateToken(It.Is<UserModel>(x =>
                    x.Roles[0].Equals(Role.Service))))
                .Returns(token);
            _httpClientWrapperMock
                .Setup(x => x.GetByIdAsync<string>("User/Username",
                    It.Is<string>(x => x.Equals(username)), token))
                .Returns(Task.FromResult(new AutoWrapperResponseModel<string> {Result = userId}));

            await _systemUnderTest.GetUserId(username);

            _tokenMock.Verify(x => x.GenerateToken(It.Is<UserModel>(x =>
                x.Roles[0].Equals(Role.Service))), Times.Once);
            _httpClientWrapperMock.Verify(x =>
                    x.GetByIdAsync<string>("User/Username", username, token),
                Times.Once);
        }

        [Test]
        public async Task GetUserId_GetByIdAsyncResult()
        {
            const string username = "someUsername";
            const string token = "someToken";
            const string userId = "someUserId";

            _tokenMock.Setup(x => x.GenerateToken(It.Is<UserModel>(x =>
                    x.Roles[0].Equals(Role.Service))))
                .Returns("SomeOtherToken");
            _httpClientWrapperMock
                .Setup(x =>
                    x.GetByIdAsync<string>("User/Username",
                        It.Is<string>(x => x.Equals(username)), token))
                .Returns(Task.FromResult(new AutoWrapperResponseModel<string>
                    {Result = userId}));

            var result = await _systemUnderTest.GetUserId(username, token);

            Assert.AreEqual(userId, result);
        }

        [Test]
        public async Task GetUserInformation_Calls_PostWithResultAsync_WithExpectedParametersAsync()
        {
            var userModel = new UserModel {Username = "SomeUsername", Password = "SomePassword"};
            const string token = "someToken";
            const string userId = "someUserId";

            _tokenMock.Setup(x => x.GenerateToken(
                    It.Is<UserModel>(x => x.Roles[0].Equals(Role.Service))))
                .Returns("SomeOtherToken");
            _httpClientWrapperMock.Setup(x =>
                    x.GetByIdAsync("User", It.Is<string>(x => x.Equals(userId)), token))
                .Returns(Task.FromResult(new AutoWrapperResponseModel<UserModel>
                    {Result = userModel}));

            await _systemUnderTest.GetUserInformation(userId, token);

            _httpClientWrapperMock.Verify(x =>
                    x.GetByIdAsync("User", It.Is<string>(x => x.Equals(userId)), token),
                Times.Once);
        }

        [Test]
        public async Task GetUserInformation_Calls_PostWithResultAsync_WithBaseClassTokenForParameterAsync()
        {
            var userModel = new UserModel {Username = "SomeUsername", Password = "SomePassword"};
            const string token = "someToken";
            const string userId = "someUserId";

            _tokenMock.Setup(x => x.GenerateToken(It.Is<UserModel>(x =>
                x.Roles[0].Equals(Role.Service)))).Returns(token);
            _httpClientWrapperMock.Setup(x =>
                    x.GetByIdAsync("User", It.Is<string>(x => x.Equals(userId)), token))
                .Returns(Task.FromResult(new AutoWrapperResponseModel<UserModel> {Result = userModel}));

            await _systemUnderTest.GetUserInformation(userId);

            _tokenMock.Verify(x => x.GenerateToken(It.Is<UserModel>(x =>
                x.Roles[0].Equals(Role.Service))), Times.Once);
            _httpClientWrapperMock.Verify(
                x => x.GetByIdAsync("User",
                    It.Is<string>(x => x.Equals(userId)), token), Times.Once);
        }

        [Test]
        public async Task GetUserInformation_ReturnsTrue_If_PostWithResultAsync_ReturnsTrue()
        {
            var userModel = new UserModel {Username = "SomeUsername", Password = "SomePassword"};
            const string token = "someToken";
            const string userId = "someUserId";

            _tokenMock.Setup(x => x.GenerateToken(It.Is<UserModel>(x =>
                    x.Roles[0].Equals(Role.Service))))
                .Returns("SomeOtherToken");
            _httpClientWrapperMock.Setup(x => x.GetByIdAsync("User",
                    It.Is<string>(x => x.Equals(userId)), token))
                .Returns(Task.FromResult(new AutoWrapperResponseModel<UserModel> {Result = userModel}));

            var result = await _systemUnderTest.GetUserInformation(userId, token);

            Assert.AreEqual(userModel.Username, result.Username);
            Assert.AreEqual(userModel.Password, result.Password);
        }
    }
}