using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ActivosFijosWeb;

namespace ActivosFijosWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalculosDepreciacionController : ControllerBase
{
    private readonly AppDbContext _context;

    public CalculosDepreciacionController()
    {
        _context = new AppDbContext();
        _context.Database.EnsureCreated();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CalculoDepreciacion>>> GetHistorico()
    {
        return await _context.CalculosDepreciacion
                             .Include(c => c.ActivoFijo)
                             .OrderByDescending(c => c.FechaProceso)
                             .ToListAsync();
    }

    [HttpPost("procesar")]
    public async Task<IActionResult> ProcesarDepreciacion(int año, int mes, int? activoId)
    {
        try
        {
            if (mes < 1 || mes > 12)
                return BadRequest(new { mensaje = "El mes debe estar entre 1 y 12." });

            if (año < 2000 || año > 2100)
                return BadRequest(new { mensaje = "El año no es válido." });

            var activos = await ObtenerActivos(activoId);
            if (!activos.Any()) return BadRequest(new { mensaje = "No se encontraron activos para procesar." });

            var nuevosCalculos = new List<CalculoDepreciacion>();
            var omitidos = new List<object>();

            foreach (var activo in activos)
            {
                var (calculo, motivo) = await ProcesarActivoPeriodo(activo, año, mes);
                if (calculo != null) nuevosCalculos.Add(calculo);
                else omitidos.Add(new { activo = activo.Descripcion, motivo });
            }

            if (!nuevosCalculos.Any())
                return BadRequest(new { mensaje = "No se generó ningún cálculo nuevo.", omitidos });

            _context.CalculosDepreciacion.AddRange(nuevosCalculos);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = $"Proceso completado. {nuevosCalculos.Count} registros generados.",
                omitidos
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al procesar la depreciación: " + ex.Message });
        }
    }

    [HttpPost("procesarAnio")]
    public async Task<IActionResult> ProcesarDepreciacionAnual(int año, int? activoId)
    {
        try
        {
            if (año < 2000 || año > 2100)
                return BadRequest(new { mensaje = "El año no es válido." });

            var activos = await ObtenerActivos(activoId);
            if (!activos.Any()) return BadRequest(new { mensaje = "No se encontraron activos para procesar." });

            var detallePorMes = new List<object>();
            int totalGenerados = 0;

            for (int mes = 1; mes <= 12; mes++)
            {
                var nuevosCalculos = new List<CalculoDepreciacion>();
                var omitidosMes = new List<object>();

                foreach (var activo in activos)
                {
                    var (calculo, motivo) = await ProcesarActivoPeriodo(activo, año, mes);
                    if (calculo != null) nuevosCalculos.Add(calculo);
                    else omitidosMes.Add(new { activo = activo.Descripcion, motivo });
                }

                if (nuevosCalculos.Any())
                {
                    _context.CalculosDepreciacion.AddRange(nuevosCalculos);
                    // Importante: se guarda mes por mes, para que la validación de
                    // secuencia del siguiente mes vea este ya procesado en la BD.
                    await _context.SaveChangesAsync();
                    totalGenerados += nuevosCalculos.Count;
                }

                detallePorMes.Add(new { mes, generados = nuevosCalculos.Count, omitidos = omitidosMes });
            }

            return Ok(new
            {
                mensaje = $"Proceso anual {año} completado. {totalGenerados} registros generados en total.",
                detallePorMes
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al procesar la depreciación anual: " + ex.Message });
        }
    }

    private async Task<List<ActivoFijo>> ObtenerActivos(int? activoId)
    {
        var query = _context.ActivosFijos.Include(a => a.TipoActivo).AsQueryable();
        if (activoId.HasValue && activoId > 0)
            query = query.Where(a => a.Id == activoId);
        return await query.ToListAsync();
    }

    private async Task<(CalculoDepreciacion? calculo, string? motivo)> ProcesarActivoPeriodo(ActivoFijo activo, int año, int mes)
    {
        int periodoSolicitado = año * 12 + mes;

        var yaProcesado = await _context.CalculosDepreciacion
            .AnyAsync(c => c.ActivoFijoId == activo.Id && c.AñoProceso == año && c.MesProceso == mes);
        if (yaProcesado)
            return (null, "Ya fue procesado en este período.");

        int periodoRegistro = activo.FechaRegistro.Year * 12 + activo.FechaRegistro.Month;
        if (periodoSolicitado < periodoRegistro)
            return (null, $"El activo fue registrado en {activo.FechaRegistro:MM/yyyy}, posterior al período solicitado.");

        var ultimo = await _context.CalculosDepreciacion
            .Where(c => c.ActivoFijoId == activo.Id)
            .OrderByDescending(c => c.AñoProceso).ThenByDescending(c => c.MesProceso)
            .FirstOrDefaultAsync();

        if (ultimo != null)
        {
            int periodoUltimo = ultimo.AñoProceso * 12 + ultimo.MesProceso;
            if (periodoSolicitado != periodoUltimo + 1)
            {
                int sigMes = ultimo.MesProceso == 12 ? 1 : ultimo.MesProceso + 1;
                int sigAño = ultimo.MesProceso == 12 ? ultimo.AñoProceso + 1 : ultimo.AñoProceso;
                return (null, $"Debe procesar primero {sigMes:00}/{sigAño} antes que este período.");
            }
        }
        else if (periodoSolicitado != periodoRegistro)
        {
            return (null, $"El primer cálculo de este activo debe ser {activo.FechaRegistro:MM/yyyy}.");
        }

        if (activo.DepreciacionAcumulada >= activo.ValorCompra)
            return (null, "El activo ya está completamente depreciado.");

        int vidaUtil = activo.TipoActivo?.VidaUtilMeses > 0 ? activo.TipoActivo.VidaUtilMeses : 48;
        decimal montoMensual = activo.ValorCompra / vidaUtil;

        if (activo.DepreciacionAcumulada + montoMensual > activo.ValorCompra)
            montoMensual = activo.ValorCompra - activo.DepreciacionAcumulada;

        activo.DepreciacionAcumulada += montoMensual;

        var calculo = new CalculoDepreciacion
        {
            AñoProceso = año,
            MesProceso = mes,
            ActivoFijoId = activo.Id,
            FechaProceso = DateTime.Now,
            MontoDepreciado = Math.Round(montoMensual, 2),
            DepreciacionAcumulada = Math.Round(activo.DepreciacionAcumulada, 2),
            CuentaCompra = activo.TipoActivo?.CuentaContableCompra ?? "N/A",
            CuentaDepreciacion = activo.TipoActivo?.CuentaContableDepreciacion ?? "N/A"
        };

        return (calculo, null);
    }
}