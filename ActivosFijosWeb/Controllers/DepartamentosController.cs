using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ActivosFijosWeb; 

namespace ActivosFijosWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartamentosController : ControllerBase
{
    private readonly AppDbContext _context;

    public DepartamentosController()
    {
        _context = new AppDbContext();
        _context.Database.EnsureCreated();
    }

    // 1. LEER TODO (GET: api/departamentos) 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Departamento>>> GetDepartamentos()
    {
        return await _context.Departamentos.ToListAsync();
    }

    // 2. LEER POR ID (GET: api/departamentos/5)
    [HttpGet("{id}")]
    public async Task<ActionResult<Departamento>> GetDepartamento(int id)
    {
        var departamento = await _context.Departamentos.FindAsync(id);
        if (departamento == null) return NotFound(new { mensaje = "Departamento no encontrado" });
        return departamento;
    }

    // 3. CREAR (POST: api/departamentos)
    [HttpPost]
    public async Task<ActionResult<Departamento>> PostDepartamento(Departamento departamento)
    {
        _context.Departamentos.Add(departamento);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDepartamento), new { id = departamento.Id }, departamento);
    }

    // 4. EDITAR (PUT: api/departamentos/5)
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDepartamento(int id, Departamento departamento)
    {
        if (id != departamento.Id) return BadRequest();

        _context.Entry(departamento).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Departamentos.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    // 5. ELIMINAR (DELETE: api/departamentos/5)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDepartamento(int id)
    {
        var departamento = await _context.Departamentos.FindAsync(id);
        if (departamento == null) return NotFound();

        _context.Departamentos.Remove(departamento);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}