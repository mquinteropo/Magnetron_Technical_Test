namespace MagnetronTecnicalTest.Dtos;

public record PersonaDto(long Id, string Nombre, string Apellido, string TipoDocumento, string Documento);
public record CreatePersonaDto(string Nombre, string Apellido, string TipoDocumento, string Documento);
