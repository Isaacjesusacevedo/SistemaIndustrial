using SistemaIndustrial.BR.DTOs;

namespace SistemaIndustrial.BR.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ServiceResult<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ServiceResult> LogoutAsync(int userId);
    Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<ServiceResult<UserInfo?>> GetUserByIdAsync(int id);
    Task<ServiceResult<bool>> ValidateTokenAsync(string token);
}