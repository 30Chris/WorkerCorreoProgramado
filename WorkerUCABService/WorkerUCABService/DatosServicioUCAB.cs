using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerUCABService
{
    internal class DatosServicioUCAB
    {

        public string AuthServi { get; set; }
        public string PassServi { get; set; }


        public DatosServicioUCAB()
        {
            AuthServi = string.Empty;
            PassServi = string.Empty;
          
        }

    }
}
