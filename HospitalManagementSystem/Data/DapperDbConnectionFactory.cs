using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HospitalManagementSystem.Data
{
    // Clase que solo se encarga de crear conexiones de BD para Dapper
    public class DapperDbConnectionFactory
    {
        private readonly string _connectionString;

        // 1. Eliminar el parámetro 'string connectionString' del constructor.
        public DapperDbConnectionFactory(IConfiguration configuration)
        {
            // Solo se necesita IConfiguration para obtener la cadena de conexión.
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no se encontró en la configuración.");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
