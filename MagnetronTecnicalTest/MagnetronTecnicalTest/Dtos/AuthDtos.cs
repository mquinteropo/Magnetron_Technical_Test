namespace MagnetronTecnicalTest.Dtos;

public record RegisterDto(string Username, string Password);
public record LoginDto(string Username, string Password);
public record AuthResultDto(string Token, DateTime ExpiresAt, string Username, string Role);
