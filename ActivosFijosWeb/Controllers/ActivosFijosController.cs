using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ActivosFijosWeb;

namespace ActivosFijosWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivosFijosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ActivosFijosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivoFijo>>> GetActivosFijos()
    {
        return await _context.ActivosFijos
                             .Include(a => a.Departamento)
                             .Include(a => a.TipoActivo)
                             .Include(a => a.Empleado)
                             .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ActivoFijo>> GetActivoFijo(int id)
    {
        var activo = await _context.ActivosFijos
                                   .Include(a => a.Departamento)
                                   .Include(a => a.TipoActivo)
                                   .Include(a => a.Empleado)
                                   .FirstOrDefaultAsync(a => a.Id == id);
        if (activo == null) return NotFound();
        return activo;
    }

    [HttpPost]
    public async Task<ActionResult<ActivoFijo>> PostActivoFijo(ActivoFijo activoFijo)
    {
        _context.ActivosFijos.Add(activoFijo);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetActivoFijo), new { id = activoFijo.Id }, activoFijo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutActivoFijo(int id, ActivoFijo activoFijo)
    {
        if (id != activoFijo.Id) return BadRequest();
        _context.Entry(activoFijo).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivoFijo(int id)
    {
        var activo = await _context.ActivosFijos.FindAsync(id);
        if (activo == null) return NotFound();
        _context.ActivosFijos.Remove(activo);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}