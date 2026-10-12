namespace SistemaIndustrial.BR.DTOs;

public record CategoriaCreateDto(string Nombre, int Orden);
public record CategoriaUpdateDto(string Nombre, int Orden, bool Activa);
public record CategoriaResponse(int Id, string Nombre, int Orden, bool Activa, int ProductosCount);