using SistemaIndustrial.BR.DTOs;

namespace SistemaIndustrial.BR.Interfaces;

public interface IProductoCatalogoService
{
    Task<ServiceResult<ProductoCatalogoResponse>> CreateAsync(ProductoCatalogoCreateDto dto);
    Task<ServiceResult<ProductoCatalogoResponse>> UpdateAsync(int id, ProductoCatalogoUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<ProductoCatalogoResponse>> GetByIdAsync(int id);
    Task<ServiceResult<List<ProductoCatalogoResponse>>> GetAllAsync();
}
