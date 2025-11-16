namespace MagnetronTecnicalTest.Dtos;

public record ProductoDto(long Id, string Descripcion, string UnidadMedida, decimal Precio, decimal Costo);
public record CreateProductoDto(string Descripcion, string UnidadMedida, decimal Precio, decimal Costo);
public record UpdatePrecioProductoDto(decimal Precio, decimal Costo);
