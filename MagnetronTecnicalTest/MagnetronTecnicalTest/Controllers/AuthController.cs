using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MagnetronTecnicalTest.Data;
using MagnetronTecnicalTest.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using MagnetronTecnicalTest.Config;
using Microsoft.AspNetCore.Authorization;

namespace MagnetronTecnicalTest.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly BillingDbContext _db;
    private readonly JwtSettings _settings;
    public AuthController(BillingDbContext db, JwtSettings settings)
    {
        _db = db;
        _settings = settings;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResultDto>> Register(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Username y Password requeridos");
        if (await _db.Usuarios.AnyAsync(u => u.Username == dto.Username))
            return Conflict("Username ya existe");

        var hash = HashPassword(dto.Password);
        var user = new Usuario { Username = dto.Username.Trim(), PasswordHash = hash, Role = "user" };
        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync();
        var token = BuildToken(user);
        return Created("api/auth/me", token);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResultDto>> Login(LoginDto dto)
    {
        var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null) return Unauthorized("Credenciales inv�lidas");
        if (!VerifyPassword(dto.Password, user.PasswordHash)) return Unauthorized("Credenciales inv�lidas");
        var token = BuildToken(user);
        return Ok(token);
    }

    private AuthResultDto BuildToken(Usuario user)
    {
        // JwtSettings.Secret is stored as Base64. Decode to raw bytes for signing.
        var key = new SymmetricSecurityKey(Convert.FromBase64String(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpMinutes);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var jwt = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds
        );
        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new AuthResultDto(token, expires, user.Username, user.Role);
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string hash) => HashPassword(password) == hash;
}
