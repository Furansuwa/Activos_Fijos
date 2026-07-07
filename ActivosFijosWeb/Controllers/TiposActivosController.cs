using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ActivosFijosWeb;

namespace ActivosFijosWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TiposActivosController : ControllerBase
{
    private readonly AppDbContext _context;

    public TiposActivosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TipoActivo>>> GetTiposActivos()
    {
        return await _context.TiposActivos.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TipoActivo>> GetTipoActivo(int id)
    {
        var tipo = await _context.TiposActivos.FindAsync(id);
        if (tipo == null) return NotFound();
        return tipo;
    }

    [HttpPost]
    public async Task<ActionResult<TipoActivo>> PostTipoActivo(TipoActivo tipoActivo)
    {
        _context.TiposActivos.Add(tipoActivo);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTipoActivo), new { id = tipoActivo.Id }, tipoActivo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutTipoActivo(int id, TipoActivo tipoActivo)
    {
        if (id != tipoActivo.Id) return BadRequest();
        _context.Entry(tipoActivo).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTipoActivo(int id)
    {
        var tipo = await _context.TiposActivos.FindAsync(id);
        if (tipo == null) return NotFound();
        _context.TiposActivos.Remove(tipo);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}