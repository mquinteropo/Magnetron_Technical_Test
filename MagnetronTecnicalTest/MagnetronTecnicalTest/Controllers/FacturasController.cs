using MagnetronTecnicalTest.Data;
using MagnetronTecnicalTest.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace MagnetronTecnicalTest.Controllers;

[Authorize]
[ApiController]
[Route("api/facturas")]
public class FacturasController : ControllerBase
{
    private readonly BillingDbContext _db;
    public FacturasController(BillingDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FacturaDto>>> GetAll()
    {
        var list = await _db.Facturas
            .Include(f => f.Detalles)
            .OrderByDescending(f => f.Fecha)
            .Select(f => new FacturaDto(
                f.Id,
                f.Numero,
                f.Fecha,
                f.PersonaId,
                f.Detalles.Select(d => new FacturaDetalleDto(d.Id, d.Linea, d.Cantidad, d.ProductoId, d.UnitPrice, d.LineTotal)).ToList()
            ))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<FacturaDto>> Get(long id)
    {
        var f = await _db.Facturas.Include(x => x.Detalles).FirstOrDefaultAsync(x => x.Id == id);
        if (f == null) return NotFound();
        return new FacturaDto(f.Id, f.Numero, f.Fecha, f.PersonaId,
            f.Detalles.Select(d => new FacturaDetalleDto(d.Id, d.Linea, d.Cantidad, d.ProductoId, d.UnitPrice, d.LineTotal)).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<FacturaDto>> Create(CreateFacturaDto dto)
    {
        var errors = new List<string>();

        // Validaciones b�sicas
        if (string.IsNullOrWhiteSpace(dto.Numero)) errors.Add("Numero requerido");
        var numeroTrim = dto.Numero?.Trim();
        if (!string.IsNullOrEmpty(numeroTrim) && numeroTrim.Length > 50) errors.Add("Numero excede longitud 50");
        if (dto.Fecha == default) errors.Add("Fecha inv�lida");
        if (dto.Fecha > DateTime.UtcNow.AddMinutes(5)) errors.Add("Fecha no puede ser futura");
        if (dto.Detalles == null || dto.Detalles.Count == 0) errors.Add("Debe incluir al menos un detalle");
        if (dto.Detalles != null && dto.Detalles.Any(d => d.Cantidad <= 0)) errors.Add("Todas las cantidades deben ser > 0");
        if (dto.Detalles != null && dto.Detalles.Any(d => d.UnitPrice < 0)) errors.Add("Precio unitario no puede ser negativo");
        if (dto.Detalles != null && dto.Detalles.Any(d => d.Linea <= 0)) errors.Add("Linea debe ser > 0");

        if (errors.Count > 0)
        {
            var dict = new Dictionary<string, string[]> { { "Errors", errors.ToArray() } };
            return ValidationProblem(new ValidationProblemDetails(dict)
            {
                Title = "Errores de validaci�n",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (await _db.Facturas.AnyAsync(x => x.Numero == numeroTrim))
            return Conflict("Numero de factura ya existe");
        if (!await _db.Personas.AnyAsync(x => x.Id == dto.PersonaId))
            return BadRequest("Persona no existe");

        // Validar lineas �nicas y secuenciales
        var lineas = dto.Detalles.Select(d => d.Linea).ToList();
        if (lineas.Distinct().Count() != lineas.Count) return BadRequest("Lineas duplicadas en detalle");
        var maxLinea = lineas.Max();
        if (maxLinea != lineas.Count || !lineas.OrderBy(x => x).SequenceEqual(Enumerable.Range(1, lineas.Count)))
            return BadRequest("Lineas deben ser consecutivas iniciando en 1");

        // Validar productos
        var productoIds = dto.Detalles.Select(x => x.ProductoId).Distinct().ToList();
        var productos = await _db.Productos.Where(p => productoIds.Contains(p.Id)).ToListAsync();
        if (productos.Count != productoIds.Count) return BadRequest("Producto inexistente en detalle");

        // Regla opcional: UnitPrice coincide con producto.Precio
        if (dto.Detalles.Any(d => productos.First(p => p.Id == d.ProductoId).Precio != d.UnitPrice))
            return BadRequest("UnitPrice de uno o m�s productos no coincide con precio registrado");

        await using var tx = await _db.Database.BeginTransactionAsync();
        var factura = new FacturaEncabezado
        {
            Numero = numeroTrim!,
            Fecha = dto.Fecha,
            PersonaId = dto.PersonaId,
            Detalles = dto.Detalles.Select(d => new FacturaDetalle
            {
                Linea = d.Linea,
                Cantidad = d.Cantidad,
                ProductoId = d.ProductoId,
                UnitPrice = d.UnitPrice
            }).ToList()
        };

        _db.Facturas.Add(factura);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        var result = new FacturaDto(factura.Id, factura.Numero, factura.Fecha, factura.PersonaId,
            factura.Detalles.Select(d => new FacturaDetalleDto(d.Id, d.Linea, d.Cantidad, d.ProductoId, d.UnitPrice, d.LineTotal)).ToList());
        return CreatedAtAction(nameof(Get), new { id = factura.Id }, result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var f = await _db.Facturas.Include(x => x.Detalles).FirstOrDefaultAsync(x => x.Id == id);
        if (f == null) return NotFound();
        _db.Facturas.Remove(f);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
