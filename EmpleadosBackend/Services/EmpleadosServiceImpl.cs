using Grpc.Core;
using System.Threading.Tasks;
using EmpleadosBackend.Services; // Asegúrate de que este es el namespace correcto.
using Cassandra;
using Google.Protobuf.WellKnownTypes;
using System.Linq;



public class EmpleadosServiceImpl : EmpleadosService.EmpleadosServiceBase
{
    private readonly CassandraDBContext _dbContext;

    public EmpleadosServiceImpl(CassandraDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    // CrearEmpleado
    public override async Task<Empleado> CrearEmpleado(Empleado request, ServerCallContext context)
    {
        // Validación de la cédula
        if (string.IsNullOrEmpty(request.Cedula))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "La cédula no puede estar vacía"));
        }

        var insertStatement = $"INSERT INTO empleados (cedula, nombre, apellido, cargo, salario) VALUES (?, ?, ?, ?, ?)";

        await _dbContext.Session.ExecuteAsync(new SimpleStatement(insertStatement, request.Cedula, request.Nombre, request.Apellido, request.Cargo, request.Salario));

        return request;
    }


    // EditarEmpleado
    public override async Task<Empleado> EditarEmpleado(Empleado request, ServerCallContext context)
    {
        var updateStatement = $"UPDATE empleados SET nombre = ?, apellido = ?, cargo = ?, salario = ? WHERE cedula = ?";

        await _dbContext.Session.ExecuteAsync(new SimpleStatement(updateStatement, request.Nombre, request.Apellido, request.Cargo, request.Salario, request.Cedula));

        return request;
    }

    // EliminarEmpleado
    public override async Task<Empleado> EliminarEmpleado(Empleado request, ServerCallContext context)
    {
        var deleteStatement = $"DELETE FROM empleados WHERE cedula = ?";

        await _dbContext.Session.ExecuteAsync(new SimpleStatement(deleteStatement, request.Cedula));

        return request; // O considera retornar un mensaje/estado de éxito.
    }

    // BuscarEmpleadoporCedula
    public override async Task<Empleado> BuscarPorCedula(Empleado request, ServerCallContext context)
    {
        var selectStatement = $"SELECT * FROM empleados WHERE cedula = ?";

        var row = await _dbContext.Session.ExecuteAsync(new SimpleStatement(selectStatement, request.Cedula)).ConfigureAwait(false);

        var empleado = row.FirstOrDefault();
        if (empleado != null)
        {
            return new Empleado
            {
                Cedula = empleado["cedula"].ToString(),
                Nombre = empleado["nombre"].ToString(),
                Apellido = empleado["apellido"].ToString(),
                Cargo = empleado["cargo"].ToString(),
                Salario = (double)empleado["salario"]
            };
        }
        else
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Empleado con cédula {request.Cedula} no encontrado"));
        }
    }

    // ListarEmpleados
    public override async Task TraerTodos(Empty request, IServerStreamWriter<Empleado> responseStream, ServerCallContext context)
    {
        var selectStatement = $"SELECT * FROM empleados";

        var rows = await _dbContext.Session.ExecuteAsync(new SimpleStatement(selectStatement)).ConfigureAwait(false);

        foreach (var row in rows)
        {
            await responseStream.WriteAsync(new Empleado
            {
                Cedula = row["cedula"].ToString(),
                Nombre = row["nombre"].ToString(),
                Apellido = row["apellido"].ToString(),
                Cargo = row["cargo"].ToString(),
                Salario = (double)row["salario"]
            });
        }
    }

    // ContarEmpleados
    public override async Task<EmpleadoCount> ContarEmpleados(Empty request, ServerCallContext context)
    {
        var selectStatement = $"SELECT COUNT(*) AS count FROM empleados";

        var row = await _dbContext.Session.ExecuteAsync(new SimpleStatement(selectStatement)).ConfigureAwait(false);
        var count = row.FirstOrDefault()?["count"] ?? 0;

        return new EmpleadoCount
        {
            Count = (int)count
        };
    }





}
