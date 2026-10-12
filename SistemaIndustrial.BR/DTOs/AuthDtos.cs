namespace SistemaIndustrial.BR.DTOs;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string Nombre, string Role);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record RefreshTokenRequest(string RefreshToken);
public record LoginResponse(string Token, string RefreshToken, DateTime ExpiresAt, UserInfo User);
public record UserInfo(int Id, string Email, string Nombre, string Role);
public record TokenPayload(int UserId, string Email, string Role, DateTime Exp, DateTime Iat);