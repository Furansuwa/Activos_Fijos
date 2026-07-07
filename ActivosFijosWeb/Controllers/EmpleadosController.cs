using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ActivosFijosWeb;

namespace ActivosFijosWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpleadosController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmpleadosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Empleado>>> GetEmpleados()
    {
        return await _context.Empleados.Include(e => e.Departamento).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Empleado>> GetEmpleado(int id)
    {
        var empleado = await _context.Empleados.Include(e => e.Departamento)
                                               .FirstOrDefaultAsync(e => e.Id == id);
        if (empleado == null) return NotFound();
        return empleado;
    }

// Dentro de EmpleadosController.cs

[HttpPost]
public async Task<ActionResult<Empleado>> PostEmpleado(Empleado empleado)
{
    // 1. Validar Cédula/RNC antes de guardar
    var (esValido, mensajeError) = ValidarIdentificacion(empleado.Cedula, empleado.TipoPersona);
    if (!esValido)
    {
        return BadRequest(new { mensaje = mensajeError });
    }

    _context.Empleados.Add(empleado);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetEmpleado), new { id = empleado.Id }, empleado);
}

[HttpPut("{id}")]
public async Task<IActionResult> PutEmpleado(int id, Empleado empleado)
{
    if (id != empleado.Id) return BadRequest();
    
    // Validar cédula/RNC en edición también
    var (esValido, mensajeError) = ValidarIdentificacion(empleado.Cedula, empleado.TipoPersona);
    if (!esValido)
    {
        return BadRequest(new { mensaje = mensajeError });
    }

    _context.Entry(empleado).State = EntityState.Modified;
    await _context.SaveChangesAsync();
    return NoContent();
}

// Decide qué validación aplicar según el tipo de persona
private static (bool ok, string mensaje) ValidarIdentificacion(string? valor, string? tipoPersona)
{
    if (string.IsNullOrWhiteSpace(valor))
        return (false, "La cédula/RNC es obligatoria.");

    if (string.Equals(tipoPersona, "Jurídica", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(tipoPersona, "Juridica", StringComparison.OrdinalIgnoreCase))
    {
        // Persona Jurídica se identifica con RNC (9 dígitos)
        string soloDigitos = valor.Replace("-", "").Replace(" ", "").Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(soloDigitos, @"^\d{9}$"))
            return (false, "El RNC debe tener 9 dígitos numéricos.");
        return (true, "");
    }

    // Persona Física: cédula dominicana (11 dígitos)
    if (!ValidaCedula(valor))
        return (false, "La cédula dominicana ingresada no es válida.");

    return (true, "");
}

public static bool ValidaCedula(string pCedula)
{
    string vcCedula = (pCedula ?? "").Replace("-", "").Replace(" ", "").Trim();

    // Debe tener EXACTAMENTE 11 dígitos numéricos (evita el crash con letras/espacios)
    if (!System.Text.RegularExpressions.Regex.IsMatch(vcCedula, @"^\d{11}$"))
        return false;

    int vnTotal = 0;
    int[] digitoMult = new int[11] { 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1 };

    for (int vDig = 1; vDig <= 11; vDig++)
    {
        int vCalculo = (vcCedula[vDig - 1] - '0') * digitoMult[vDig - 1];
        vnTotal += vCalculo < 10 ? vCalculo : (vCalculo / 10) + (vCalculo % 10);
    }

    return vnTotal % 10 == 0;
}
}