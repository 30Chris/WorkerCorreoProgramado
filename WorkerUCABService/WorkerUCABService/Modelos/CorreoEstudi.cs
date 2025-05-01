using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerUCABService.Modelos
{
    public class CorreoEstudi
    {
        public class Rootobject
        {
            public int count { get; set; }
            public object next { get; set; }
            public object previous { get; set; }
            public Result[] results { get; set; }

           
        }

      

        public class Result
        {
            public string email_address_code { get; set; }
            public string email_address_type { get; set; }
            public string email_address { get; set; }
            public bool primary { get; set; }
            public DateTime last_activity { get; set; }
        }

    }
}
