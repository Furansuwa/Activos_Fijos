using ActivosFijosWeb;

var builder = WebApplication.CreateBuilder(args);

// 1. Soporte para Controladores
builder.Services.AddControllers();

// 2. Registro de Base de Datos
builder.Services.AddDbContext<AppDbContext>();

// 3. Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Esto permite que tu archivo index.html se conecte a la API sin bloqueos de seguridad
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

/*Si cualquier endpoint lanza un error no controlado, esto evita que la
conexión se cierre "en seco" (lo que el navegador ve como fallo de red)
 y en su lugar devuelve un JSON con el error real.*/
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine("ERROR NO CONTROLADO: " + ex);
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"mensaje\":\"Error interno: {ex.Message.Replace("\"", "'")}\"}}");
        }
    }
});

// 4. Activar Swagger (siempre, para facilitar tus pruebas de universidad)
app.UseSwagger();
app.UseSwaggerUI();

// Activar CORS (Importante: debe ir antes de Authorization) ---
app.UseCors("PermitirTodo");

app.UseAuthorization();

// 5. Mapeo de rutas de controladores
app.MapControllers();

// Redirección automática a Swagger al abrir el sitio ---
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();