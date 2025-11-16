namespace MagnetronTecnicalTest.Dtos;

public record FacturaDto(long Id, string Numero, DateTime Fecha, long PersonaId, List<FacturaDetalleDto> Detalles);
public record CreateFacturaDto(string Numero, DateTime Fecha, long PersonaId, List<CreateFacturaDetalleDto> Detalles);
public record FacturaDetalleDto(long Id, int Linea, decimal Cantidad, long ProductoId, decimal UnitPrice, decimal LineTotal);
public record CreateFacturaDetalleDto(int Linea, decimal Cantidad, long ProductoId, decimal UnitPrice);
