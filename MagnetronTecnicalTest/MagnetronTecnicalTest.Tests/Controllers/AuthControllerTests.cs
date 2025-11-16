using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MagnetronTecnicalTest.Controllers;
using MagnetronTecnicalTest.Dtos;
using MagnetronTecnicalTest.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace MagnetronTecnicalTest.Tests.Controllers;

public class AuthControllerTests : TestBase
{
    private AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(Context, JwtSettings);
        SeedAuthData();
    }

    private void SeedAuthData()
    {
        // Crear un usuario con password hasheado usando SHA256 (como en el controlador real)
        var hashedPassword = HashPassword("testpassword");
        Context.Usuarios.Add(new Usuario 
        { 
            Id = 10, 
            Username = "existinguser", 
            PasswordHash = hashedPassword, 
            Role = "user" 
        });
        Context.SaveChanges();
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var dto = new RegisterDto("newuser", "password123");

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Result.Should().BeOfType<CreatedResult>();
        var createdResult = result.Result as CreatedResult;
        var authResult = createdResult!.Value as AuthResultDto;
        authResult.Should().NotBeNull();
        authResult!.Username.Should().Be("newuser");
        authResult.Token.Should().NotBeNullOrEmpty();
        authResult.Role.Should().Be("user");
    }

    [Fact]
    public async Task Register_WithEmptyUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new RegisterDto("", "password123");

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_WithEmptyPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new RegisterDto("testuser", "");

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_WithExistingUsername_ShouldReturnConflict()
    {
        // Arrange
        var dto = new RegisterDto("existinguser", "password123");

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var dto = new LoginDto("existinguser", "testpassword");

        // Act
        var result = await _controller.Login(dto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var authResult = okResult!.Value as AuthResultDto;
        authResult.Should().NotBeNull();
        authResult!.Username.Should().Be("existinguser");
        authResult.Token.Should().NotBeNullOrEmpty();
        authResult.Role.Should().Be("user");
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ShouldReturnUnauthorized()
    {
        // Arrange
        var dto = new LoginDto("nonexistentuser", "password123");

        // Act
        var result = await _controller.Login(dto);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var dto = new LoginDto("existinguser", "wrongpassword");

        // Act
        var result = await _controller.Login(dto);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Register_ShouldTrimUsername()
    {
        // Arrange
        var dto = new RegisterDto("  trimuser  ", "password123");

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Result.Should().BeOfType<CreatedResult>();
        
        // Verify the username was trimmed in the database
        var user = await Context.Usuarios.FirstOrDefaultAsync(u => u.Username == "trimuser");
        user.Should().NotBeNull();
        user!.Username.Should().Be("trimuser");
    }

    [Theory]
    [InlineData(null, "password")]
    [InlineData("username", null)]
    [InlineData("   ", "password")]
    [InlineData("username", "   ")]
    public async Task Register_WithNullOrWhitespaceData_ShouldReturnBadRequest(string? username, string? password)
    {
        // Arrange
        var dto = new RegisterDto(username!, password!);

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_TokenShouldHaveCorrectClaims()
    {
        // Arrange
        var dto = new LoginDto("existinguser", "testpassword");

        // Act
        var result = await _controller.Login(dto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var authResult = okResult!.Value as AuthResultDto;
        
        // Verify token properties
        authResult.Should().NotBeNull();
        authResult!.Token.Should().NotBeNullOrEmpty();
        authResult.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        authResult.Username.Should().Be("existinguser");
        authResult.Role.Should().Be("user");
    }
}