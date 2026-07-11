using System;
using System.ComponentModel.DataAnnotations;

namespace ActivosFijosWeb;

public class Departamento
{
    public int Id { get; set; } // Identificador 
    
    [Required]
    public string Descripcion { get; set; } = string.Empty; // Descripción 
    public bool Estado { get; set; } = true; // Estado 
}

public class TipoActivo
{
    public int Id { get; set; } // Identificador 
    
    [Required]
    public string Descripcion { get; set; } = string.Empty; // Descripción 
    
    [Required]
    public string CuentaContableCompra { get; set; } = string.Empty; // Cuenta Contable Compra 
    
    [Required]
    public string CuentaContableDepreciacion { get; set; } = string.Empty; // Cuenta Contable Depreciación 

    public int VidaUtilMeses { get; set; } = 48; // Vida útil en meses (ej: Mobiliario=120, Transporte=60, Edificio=240)
    public bool Estado { get; set; } = true; // Estado 
}

public class Empleado
{
    public int Id { get; set; } // Identificador 
    
    [Required]
    public string Nombre { get; set; } = string.Empty; // Nombre 
    
    [Required]
    public string Cedula { get; set; } = string.Empty; // Cédula 
    
    public int DepartamentoId { get; set; } // Departamento (Relación) 
    public Departamento? Departamento { get; set; }
    
    public string TipoPersona { get; set; } = "Física"; // Física / Jurídica 
    public DateTime FechaIngreso { get; set; } = DateTime.Now; // Fecha de Ingreso 
    public bool Estado { get; set; } = true; // Estado 
}

public class ActivoFijo
{
    public int Id { get; set; } // Identificador 
    
    [Required]
    public string Descripcion { get; set; } = string.Empty; // Descripción 
    
    public int DepartamentoId { get; set; } // Departamento (Relación) 
    public Departamento? Departamento { get; set; }
    
    public int TipoActivoId { get; set; } // Tipo Activo (Relación) 
    public TipoActivo? TipoActivo { get; set; }
    
    public int? EmpleadoId { get; set; } // Responsable/Custodio del activo (Relación, opcional)
    public Empleado? Empleado { get; set; }
    
    public DateTime FechaRegistro { get; set; } = DateTime.Now; // Fecha de Registro 
    public decimal ValorCompra { get; set; } // Valor Compra 
    public decimal DepreciacionAcumulada { get; set; } = 0m; // Depreciación Acumulada 
}

public class CalculoDepreciacion
{
    public int Id { get; set; }
    public int AñoProceso { get; set; }
    public int MesProceso { get; set; }
    public int ActivoFijoId { get; set; }
    public ActivoFijo? ActivoFijo { get; set; }
    public DateTime FechaProceso { get; set; } = DateTime.Now;
    public decimal MontoDepreciado { get; set; }
    public decimal DepreciacionAcumulada { get; set; }
    public string CuentaCompra { get; set; } = string.Empty;
    public string CuentaDepreciacion { get; set; } = string.Empty;
}