using Microsoft.EntityFrameworkCore;

namespace ActivosFijosWeb;

public class AppDbContext : DbContext
{
    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<TipoActivo> TiposActivos => Set<TipoActivo>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<ActivoFijo> ActivosFijos => Set<ActivoFijo>();
    
    public DbSet<CalculoDepreciacion> CalculosDepreciacion => Set<CalculoDepreciacion>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=activos_web.db");
    }
}