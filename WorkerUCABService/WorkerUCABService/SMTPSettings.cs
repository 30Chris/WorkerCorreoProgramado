using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerUCABService
{
    public class SMTPSettings
    {

        public string ServidorSMTP { get; set; }
        public string puertoSMTP { get; set; }
        public int Sender { get; set; }
        public string CopiaCorreo { get; set; }
        public string ImagenHead { get; set; }



        public SMTPSettings()
        {
            ServidorSMTP = string.Empty;
            puertoSMTP = string.Empty;
            Sender = 0;
            CopiaCorreo = string.Empty;
            ImagenHead = string.Empty;
        }

    }
}
