using EmpleadosBackend.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configura CORS para permitir cualquier origen, cualquier método, y cualquier encabezado.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

// Asumiendo que CassandraDBContext toma una cadena de conexión y un nombre de keyspace como parámetros.
builder.Services.AddScoped<CassandraDBContext>(serviceProvider => new CassandraDBContext("127.0.0.1", "empresa_demo"));

// Añade servicios al contenedor, incluyendo gRPC y tu contexto de Cassandra.
builder.Services.AddGrpc().AddJsonTranscoding();


var app = builder.Build();

// Usa la política CORS en tu aplicación.
app.UseCors("AllowAll");

// Configura el pipeline de solicitudes HTTP.
app.MapGrpcService<EmpleadosServiceImpl>();
app.MapGet("/", () => "El servidor gRPC está corriendo.");

app.Run();
