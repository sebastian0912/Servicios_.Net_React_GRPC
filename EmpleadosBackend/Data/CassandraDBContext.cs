using Cassandra;
using Cassandra.Mapping;
using EmpleadosModel;

public class CassandraDBContext
{
    public Cassandra.ISession Session { get; private set; }
    public CassandraDBContext(string connectionString, string keyspace)
    {
        Cluster cluster = Cluster.Builder().AddContactPoint(connectionString).Build();
        Session = cluster.Connect(keyspace);

        // Configura los mapeos aquí si es necesario
        MappingConfiguration.Global.Define(
            new Map<Empleado>()
                .TableName("empleados")
                .PartitionKey(u => u.Cedula)
                .Column(u => u.Cedula, cm => cm.WithName("cedula"))
                .Column(u => u.Nombre, cm => cm.WithName("nombre"))
                .Column(u => u.Apellido, cm => cm.WithName("apellido"))
                .Column(u => u.Cargo, cm => cm.WithName("cargo"))
                .Column(u => u.Salario, cm => cm.WithName("salario"))
        );
    }
}
