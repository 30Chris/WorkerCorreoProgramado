using Newtonsoft.Json.Linq;
using System.Net;

namespace WorkerUCABService.Procesos
{
    public class ServiciosUCAB
    {

        Logs l = new Logs();

        private readonly IConfiguration _configuration;
        private readonly string _CorreosUse;
        private readonly string _ComplementoCorreo;
        private readonly string _AuthServi;
        private readonly string _PassServi;
       
        public ServiciosUCAB(IConfiguration configuration)
        {

            _configuration = configuration;
            _CorreosUse = _configuration["ServiciosUCAB:CorreosUse"] ?? "default_sender@example.com";
            _ComplementoCorreo = _configuration["ServiciosUCAB:ComplementoCorreo"] ?? string.Empty;
            _AuthServi = _configuration["DatosServicio:AuthServi"] ?? string.Empty;
            _PassServi = _configuration["DatosServicio:PassServi"] ?? string.Empty;
           
        }

        //Trae los datos de correo de un usuario
        public async Task<string> Correos(string IDCedula)
        {
            string respuesta = "";

            try
            {
                respuesta = await ConsultaServicioGET($"{_CorreosUse}{IDCedula}{_ComplementoCorreo}");
            }
            catch (Exception ex)
            {
                l.ErrorLog(this, ex);
                throw;
            }

            return respuesta;
        }

        private HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true; // Aceptar certificados autofirmados
            return new HttpClient(handler);
        }

        private async Task<string> ConsultaServicioGET(string servicio)
        {
            try
            {
                string authServi = _AuthServi;
                string passServi = _PassServi;
              

                var client = CreateHttpClient();
                var authToken = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{authServi}:{passServi}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
                client.Timeout = TimeSpan.FromMinutes(10); // 10 minutos

                HttpResponseMessage response = await client.GetAsync(servicio);
                response.EnsureSuccessStatusCode(); // Lanza una excepción si el código de estado HTTP no está en el rango 2xx

                string content = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(content);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return string.Empty;
                }
                else
                {
                    return jObject.ToString();
                }
            }
            catch (Exception ex)
            {
                l.ErrorLog(this, ex);
                throw;
            }
        }


    }

}
