using MagnetronTecnicalTest.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MagnetronTecnicalTest.Controllers;

[Authorize]
[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly BillingDbContext _db;
    public ReportesController(BillingDbContext db) => _db = db;

    [HttpGet("personas-total")]
    public async Task<IActionResult> PersonasTotal()
    {
        var data = await _db.PersonaTotales.AsNoTracking().ToListAsync();
        return Ok(data);
    }

    [HttpGet("persona-producto-mas-caro")]
    public async Task<IActionResult> PersonaProductoMasCaro()
    {
        var data = await _db.PersonaProductoMasCaro.AsNoTracking().ToListAsync();
        return Ok(data);
    }

    [HttpGet("productos-cantidad-desc")]
    public async Task<IActionResult> ProductosPorCantidadDesc()
    {
        var data = await _db.ProductosPorCantidad.AsNoTracking().OrderByDescending(x => x.CantidadFacturada).ToListAsync();
        return Ok(data);
    }

    [HttpGet("productos-utilidad-desc")]
    public async Task<IActionResult> ProductosPorUtilidadDesc()
    {
        var data = await _db.ProductosPorUtilidad.AsNoTracking().OrderByDescending(x => x.UtilidadTotal).ToListAsync();
        return Ok(data);
    }

    [HttpGet("productos-margen")]
    public async Task<IActionResult> ProductosMargen()
    {
        var data = await _db.ProductosMargen.AsNoTracking().ToListAsync();
        return Ok(data);
    }
}
