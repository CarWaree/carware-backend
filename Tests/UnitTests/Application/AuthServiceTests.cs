using AutoMapper;
using CarWare.Application.Common.helper;
using CarWare.Application.Common.Security;
using CarWare.Application.DTOs.Auth;
using CarWare.Application.Interfaces;
using CarWare.Application.Services;
using CarWare.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace Tests.UnitTesting.Application
{
    public class AuthServiceTests
    {
        // ─── Mocks ───
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;

        private readonly Mock<IOtpGenerator> _otpGeneratorMock;
        private readonly Mock<IRefreshTokenGenerator> _refreshTokenMock;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenMock;

        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;

        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _userManagerMock = CreateUserManagerMock();
            _signInManagerMock = CreateSignInManagerMock(_userManagerMock);

            _otpGeneratorMock = new Mock<IOtpGenerator>();
            _refreshTokenMock = new Mock<IRefreshTokenGenerator>();
            _jwtTokenMock = new Mock<IJwtTokenGenerator>();

            _cacheMock = new Mock<IDistributedCache>();
            _configMock = new Mock<IConfiguration>();
            _emailSenderMock = new Mock<IEmailSender>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            // ─── SUT ───
            _sut = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _otpGeneratorMock.Object,
                _emailSenderMock.Object,
                _refreshTokenMock.Object,
                _jwtTokenMock.Object,
                _cacheMock.Object,
                _configMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        // ─── Helpers ───
        private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private static Mock<SignInManager<ApplicationUser>> CreateSignInManagerMock(
            Mock<UserManager<ApplicationUser>> userManagerMock)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

            return new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null, null, null, null);
        }

        // ─── Register ───

        [Fact]
        public async Task RegisterAsync_ShouldFail_WhenUsernameAlreadyExists()
        {
            var dto = new RegisterDto
            {
                Username = "takenUser",
                Email = "new@mail.com",
                Password = "Pass@123"
            };

            _userManagerMock.Setup(x => x.FindByNameAsync(dto.Username))
                .ReturnsAsync(new ApplicationUser());

            var result = await _sut.RegisterAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Username is already taken", result.Error);
        }

        [Fact]
        public async Task RegisterAsync_ShouldFail_WhenEmailAlreadyExists()
        {
            var dto = new RegisterDto
            {
                Username = "newUser",
                Email = "taken@mail.com",
                Password = "Pass@123"
            };

            _userManagerMock.Setup(x => x.FindByNameAsync(dto.Username))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(new ApplicationUser());

            var result = await _sut.RegisterAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Email already registered", result.Error);
        }

        [Fact]
        public async Task RegisterAsync_ShouldFail_WhenUserCreationFails()
        {
            var dto = new RegisterDto
            {
                Username = "newUser",
                Email = "new@mail.com",
                Password = "weak"
            };

            var user = new ApplicationUser();

            _userManagerMock.Setup(x => x.FindByNameAsync(dto.Username))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser)null);

            _mapperMock.Setup(m => m.Map<ApplicationUser>(dto))
                .Returns(user);

            _userManagerMock.Setup(x => x.CreateAsync(user, dto.Password))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Description = "Password too weak" }));

            var result = await _sut.RegisterAsync(dto);

            Assert.False(result.Success);
            Assert.Contains("Password too weak", result.Error);
        }

        [Fact]
        public async Task RegisterAsync_ShouldSucceed_AndSendOtp_WhenDataIsValid()
        {
            var dto = new RegisterDto
            {
                Username = "newUser",
                Email = "new@mail.com",
                Password = "Pass@123"
            };

            var user = new ApplicationUser
            {
                Id = "user-1",
                Email = dto.Email
            };

            var responseDto = new RegisterResponseDto();

            _userManagerMock.Setup(x => x.FindByNameAsync(dto.Username))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser)null);

            _mapperMock.Setup(m => m.Map<ApplicationUser>(dto))
                .Returns(user);

            _mapperMock.Setup(m => m.Map<RegisterResponseDto>(user))
                .Returns(responseDto);

            _userManagerMock.Setup(x => x.CreateAsync(user, dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(user, "User"))
                .ReturnsAsync(IdentityResult.Success);

            _otpGeneratorMock.Setup(x => x.Generate())
                .Returns("123456");

            _cacheMock.Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                default))
                .Returns(Task.CompletedTask);

            _emailSenderMock.Setup(x =>
                x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.RegisterAsync(dto);

            Assert.True(result.Success);
            Assert.False(result.Data!.IsEmailVerified);

            _emailSenderMock.Verify(x =>
                x.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        // ─── Login ───

        [Fact]
        public async Task LoginAsync_ShouldFail_WhenUserNotFound()
        {
            var dto = new LoginDto
            {
                EmailOrUsername = "test@mail.com",
                Password = "123456@pass"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.EmailOrUsername))
                .ReturnsAsync((ApplicationUser)null);

            var result = await _sut.LoginAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Invalid credentials", result.Error);
        }

        [Fact]
        public async Task LoginAsync_ShouldFail_WhenPasswordIsWrong()
        {
            // Arrange
            var dto = new LoginDto
            {
                EmailOrUsername = "test@mail.com",
                Password = "wrongpass"
            };

            var user = new ApplicationUser
            {
                Id = "1",
                Email = dto.EmailOrUsername,
                UserName = "test",
                EmailConfirmed = true
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.EmailOrUsername))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.FindByNameAsync(dto.EmailOrUsername))
                .ReturnsAsync((ApplicationUser)null);

            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(
                user,
                dto.Password,
                true))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid credentials", result.Error);
        }

        [Fact]
        public async Task LoginAsync_ShouldSucceed_WhenDataIsValid()
        {
            var user = new ApplicationUser
            {
                Email = "test@test.com",
                UserName = "test",
                FirstName = "A",
                LastName = "B",
                EmailConfirmed = true,
                RefreshTokens = new List<RefreshToken>()
            };

            var dto = new LoginDto
            {
                EmailOrUsername = "test@test.com",
                Password = "123"
            };

            var jwt = new JwtSecurityToken();

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.EmailOrUsername))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, dto.Password, true))
                .ReturnsAsync(SignInResult.Success);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            _jwtTokenMock.Setup(x => x.CreateToken(user))
                .ReturnsAsync(jwt);

            _refreshTokenMock.Setup(x => x.Generate())
                .Returns(new RefreshToken
                {
                    Token = "refresh123",
                    ExpiresOn = DateTime.UtcNow.AddDays(7)
                });

            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.LoginAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data.AccessToken);
            Assert.Equal("refresh123", result.Data.RefreshToken);
        }
    }
}