using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using BC = BCrypt.Net.BCrypt;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;

namespace SistemaIndustrial.BR.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(request.Email);
        if (usuario == null || !usuario.Activo)
            return ServiceResult<LoginResponse>.Fail("Credenciales inválidas.");

        if (!BC.Verify(request.Password, usuario.PasswordHash))
            return ServiceResult<LoginResponse>.Fail("Credenciales inválidas.");

        var token = GenerateJwtToken(usuario);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenHash = BC.HashPassword(refreshToken, 12);
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _usuarioRepository.UpdateRefreshTokenAsync(usuario.Id, refreshTokenHash, refreshTokenExpiry);

        var response = new LoginResponse(
            token,
            refreshToken,
            DateTime.UtcNow.AddHours(8),
            new UserInfo(usuario.Id, usuario.Email, usuario.Nombre, usuario.Role)
        );

        return ServiceResult<LoginResponse>.Ok(response, "Login exitoso.");
    }

    public async Task<ServiceResult<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return ServiceResult<LoginResponse>.Fail("Refresh token requerido.");

        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken? token;
        try
        {
            token = handler.ReadJwtToken(request.RefreshToken);
        }
        catch
        {
            return ServiceResult<LoginResponse>.Fail("Refresh token inválido.");
        }

        var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return ServiceResult<LoginResponse>.Fail("Refresh token inválido.");

        var usuario = await _usuarioRepository.GetByIdAsync(userId);
        if (usuario == null || !usuario.Activo)
            return ServiceResult<LoginResponse>.Fail("Usuario no encontrado.");

        if (usuario.RefreshTokenHash == null || !BC.Verify(request.RefreshToken, usuario.RefreshTokenHash))
            return ServiceResult<LoginResponse>.Fail("Refresh token inválido.");

        if (usuario.RefreshTokenExpiry <= DateTime.UtcNow)
            return ServiceResult<LoginResponse>.Fail("Refresh token expirado.");

        var newToken = GenerateJwtToken(usuario);
        var newRefreshToken = GenerateRefreshToken();
        var newRefreshTokenHash = BC.HashPassword(newRefreshToken, 12);
        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _usuarioRepository.UpdateRefreshTokenAsync(usuario.Id, newRefreshTokenHash, newRefreshTokenExpiry);

        var response = new LoginResponse(
            newToken,
            newRefreshToken,
            DateTime.UtcNow.AddHours(8),
            new UserInfo(usuario.Id, usuario.Email, usuario.Nombre, usuario.Role)
        );

        return ServiceResult<LoginResponse>.Ok(response, "Token renovado.");
    }

    public async Task<ServiceResult> LogoutAsync(int userId)
    {
        await _usuarioRepository.UpdateRefreshTokenAsync(userId, null, null);
        return ServiceResult.Ok("Logout exitoso.");
    }

    public async Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId);
        if (usuario == null)
            return ServiceResult.Fail("Usuario no encontrado.");

        if (!BC.Verify(request.CurrentPassword, usuario.PasswordHash))
            return ServiceResult.Fail("Contraseña actual incorrecta.");

        if (request.NewPassword.Length < 8)
            return ServiceResult.Fail("La nueva contraseña debe tener al menos 8 caracteres.");

        usuario.PasswordHash = BC.HashPassword(request.NewPassword, 12);
        await _usuarioRepository.UpdateAsync(userId, usuario);

        return ServiceResult.Ok("Contraseña cambiada correctamente.");
    }

    public async Task<ServiceResult<UserInfo?>> GetUserByIdAsync(int id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
            return ServiceResult<UserInfo?>.Ok(null, "Usuario no encontrado.");

        var userInfo = new UserInfo(usuario.Id, usuario.Email, usuario.Nombre, usuario.Role);
        return ServiceResult<UserInfo?>.Ok(userInfo);
    }

    public Task<ServiceResult<bool>> ValidateTokenAsync(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default-secret-key-min-32-chars-long");
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            handler.ValidateToken(token, validationParameters, out _);
            return Task.FromResult(ServiceResult<bool>.Ok(true));
        }
        catch
        {
            return Task.FromResult(ServiceResult<bool>.Ok(false));
        }
    }

    private string GenerateJwtToken(Usuario usuario)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default-secret-key-min-32-chars-long");
        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            notBefore: DateTime.UtcNow,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}