using MagnetronTecnicalTest.Data;
using MagnetronTecnicalTest.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MagnetronTecnicalTest.Controllers;

[Authorize]
[ApiController]
[Route("api/personas")]
public class PersonasController : ControllerBase
{
    private readonly BillingDbContext _db;
    public PersonasController(BillingDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PersonaDto>>> GetAll()
    {
        var data = await _db.Personas
            .Select(p => new PersonaDto(p.Id, p.Nombre, p.Apellido, p.TipoDocumento, p.Documento))
            .ToListAsync();
        return Ok(data);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<PersonaDto>> Get(long id)
    {
        var p = await _db.Personas.FindAsync(id);
        if (p == null) return NotFound();
        return new PersonaDto(p.Id, p.Nombre, p.Apellido, p.TipoDocumento, p.Documento);
    }

    [HttpPost]
    public async Task<ActionResult<PersonaDto>> Create(CreatePersonaDto dto)
    {
        if (await _db.Personas.AnyAsync(x => x.Documento == dto.Documento))
            return Conflict("Documento ya existe");
        var entity = new Persona
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            TipoDocumento = dto.TipoDocumento,
            Documento = dto.Documento
        };
        _db.Personas.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new PersonaDto(entity.Id, entity.Nombre, entity.Apellido, entity.TipoDocumento, entity.Documento));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, CreatePersonaDto dto)
    {
        var p = await _db.Personas.FindAsync(id);
        if (p == null) return NotFound();
        // Validar documento �nico si cambia
        if (p.Documento != dto.Documento && await _db.Personas.AnyAsync(x => x.Documento == dto.Documento))
            return Conflict("Documento ya existe");
        p.Nombre = dto.Nombre;
        p.Apellido = dto.Apellido;
        p.TipoDocumento = dto.TipoDocumento;
        p.Documento = dto.Documento;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var p = await _db.Personas.FindAsync(id);
        if (p == null) return NotFound();
        _db.Personas.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
