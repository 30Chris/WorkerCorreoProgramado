using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WorkerUCABService
{
    public class Conexion
    {

        private static string cadenaSql = string.Empty;

        public Conexion(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("ConexionSQL");
            if (connectionString != null)
            {
                cadenaSql = connectionString;
            }
            else
            {
                throw new ArgumentNullException(nameof(connectionString), "La cadena de conexión 'ConexionSQL' no se encontró en la configuración.");
            }
        }

        public static string CadenaSQL
        {
            get { return cadenaSql; }
        }

    }
}
