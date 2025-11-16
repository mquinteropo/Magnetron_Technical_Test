using MagnetronTecnicalTest.Data;
using MagnetronTecnicalTest.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MagnetronTecnicalTest.Controllers;

[Authorize]
[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    private readonly BillingDbContext _db;
    public ProductosController(BillingDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetAll()
    {
        var list = await _db.Productos
            .Select(p => new ProductoDto(p.Id, p.Descripcion, p.UnidadMedida, p.Precio, p.Costo))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductoDto>> Get(long id)
    {
        var p = await _db.Productos.FindAsync(id);
        if (p == null) return NotFound();
        return new ProductoDto(p.Id, p.Descripcion, p.UnidadMedida, p.Precio, p.Costo);
    }

    [HttpPost]
    public async Task<ActionResult<ProductoDto>> Create(CreateProductoDto dto)
    {
        var entity = new Producto
        {
            Descripcion = dto.Descripcion,
            UnidadMedida = dto.UnidadMedida,
            Precio = dto.Precio,
            Costo = dto.Costo
        };
        _db.Productos.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new ProductoDto(entity.Id, entity.Descripcion, entity.UnidadMedida, entity.Precio, entity.Costo));
    }

    [HttpPut("{id:long}/precios")]
    public async Task<IActionResult> UpdatePrecios(long id, UpdatePrecioProductoDto dto)
    {
        var p = await _db.Productos.FindAsync(id);
        if (p == null) return NotFound();
        p.Precio = dto.Precio;
        p.Costo = dto.Costo;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var p = await _db.Productos.FindAsync(id);
        if (p == null) return NotFound();
        _db.Productos.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
