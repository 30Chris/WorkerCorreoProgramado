#region Referencias
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using WorkerUCABService.Modelos;

#endregion



namespace WorkerUCABService.Procesos
{
    public class Correo
    {

        #region Clases
        Seguridad seg = new Seguridad();
        Logs l = new Logs();
        #endregion

            private readonly IConfiguration _configuration;
        private readonly string _sender;
        private readonly string _copiaCorreo;
        private readonly string _ImagenHead;
        private readonly string _ServidorSMTP;
        private readonly string _puertoSMTP;
        private readonly string _usSMTP;
        private readonly string _passSMTP;
        private readonly ServiciosUCAB _serviciosUCAB;
        private readonly string _Encuesta;



        public Correo(IConfiguration configuration, ServiciosUCAB serviciosUCAB)
        {
            _configuration = configuration;
            _sender = _configuration["SMTPSettings:Sender"] ?? "default_sender@example.com";
            _copiaCorreo = _configuration["SMTPSettings:CopiaCorreo"] ?? "default_copy@example.com";
            _ImagenHead = _configuration["SMTPSettings:ImagenHead"] ?? "default_copy@example.com";
            _ServidorSMTP = _configuration["SMTPSettings:ServidorSMTP"] ?? "default_copy@example.com";
            _puertoSMTP = _configuration["SMTPSettings:puertoSMTP"] ?? "default_copy@example.com";
            _usSMTP = _configuration["SMTPSettings:usSMTP"] ?? "default_copy@example.com";
            _passSMTP = _configuration["SMTPSettings:passSMTP"] ?? "default_copy@example.com";
            _serviciosUCAB = serviciosUCAB;
            _Encuesta = _configuration["SMTPSettings:puertoSMTP"] ?? "default_copy@example.com";
        }

        string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Procesos", "plantillaCorreo.html");

       

        //Conexion al Servidor SMTP con Usuario y clave
        protected void envia(MailMessage msg)
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            SmtpClient client = new()
            {
                Credentials = new NetworkCredential(_usSMTP, _passSMTP),
                Port = Convert.ToInt32(_puertoSMTP),
                Host = _ServidorSMTP,
                EnableSsl = true
            };
            try
            { client.Send(msg); }
            catch (SmtpException ex)
            { l.ErrorLog(this, ex); }
        }


        public async Task EnviaDocumentoAsync(string IdUsuario, string documento, string idDocumento, string NombreDocumento)
        {
            try
            {
                
                string Pie = "<DIV ALIGN=\"justify\"><br><br> Gracias<br><br><br><br>Este mensaje de datos y sus anexos son &uacute;nicamente para el uso exclusivo de su(s) destinatario(s). Toda revisi&oacute;n, reproducci&oacute;n, publicaci&oacute;n, distribuci&oacute;n o reenvío sin" +
                " la autorizaci&oacute;n expresa del remitente está estrictamente prohibida. Si usted recibe este correo por error, le agradecemos notificarlo de inmediato al remitente y eliminar todas las copias. Gracias<br><br>" +
                   "This email and any attachments transmitted with it are only for the use of the entity or individual to whom it is addressed. Any review, reproduction, publication, distribution or forwarding without express permission is strictly prohibited." +
                " If you receive this email in error, please notify the sender and delete all copies. Thank you.<br>" +
                "<p style=\"color:#008f39\";>CONSIDERE EL MEDIO AMBIENTE ANTES DE IMPRIMIR ESTE MENSAJE</p>" +
                   "</DIV><br>";
                string urlEncuesta = _Encuesta;
                string encuestaPie = $"Para nosotros es muy importante conocer tu opinión para así mejorar nuestros procesos, necesitamos saber cómo fue el desarrollo de tu solicitud de documentación. Esta encuesta es anónima: {urlEncuesta}<br><br>";



                seg.GeneraNonce();
              

                string CorreoEnvia = "";

                if (await BuscaCorreo(idDocumento) == "")
                {
                    string jsonResponse = await _serviciosUCAB.Correos(IdUsuario);
                    CorreoEstudi.Rootobject? tempCorreEs = JsonConvert.DeserializeObject<CorreoEstudi.Rootobject>(jsonResponse);

                    if (tempCorreEs == null)
                    {
                        // Manejar el caso cuando tempCorreEs es null
                        throw new InvalidOperationException("Error al deserializar el objeto JSON.");
                    }

                    CorreoEstudi.Rootobject correEs = tempCorreEs;

                    foreach (var i in correEs.results)
                    {
                        if (i.email_address != null || !String.IsNullOrEmpty(i.email_address))
                        {

                            CorreoEnvia = i.email_address;

                            // Convertir el documento Base64 a bytes y crear la carpeta temporal y el archivo adjunto
                            byte[] FirmadoBytes = Convert.FromBase64String(documento);
                            string CarTempo = CarpetaCrea(Path.Combine(Directory.GetCurrentDirectory(), "Docs", "Correo", seg.codNonce));
                            string adjunto = CreaArchivo(CarTempo, FirmadoBytes, NombreDocumento);
                            string titulo = "Documentos firmados";
                            string bodyCorreo = File.ReadAllText(templatePath);
                            string mensaje_html = "<br><br> Reciba un cordial saludo,<br><br>Adjunto remitimos el documento firmado por la/las autoridades respectivas de acuerdo a su solicitud.<br><br> Este documento es firmado digitalmente, por lo tanto pierde su vigencia una vez que es impreso. En caso que otra persona requiera verificar la autenticidad del mismo podr&aacute; ingresar en el portal (Pagina web) y al escanear el c&oacute;digo QR podr&aacute; visualizar los datos del mismo. <br></h4>";
                            bodyCorreo = bodyCorreo.Replace("#cuerpocorreo#", mensaje_html);
                            bodyCorreo = bodyCorreo.Replace("#PIEFIRMA#", Pie);
                            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(bodyCorreo, Encoding.UTF8, MediaTypeNames.Text.Html);

                            // Crear un nuevo objeto MailMessage en cada iteración
                            using (MailMessage mssg = new MailMessage())
                            {
                                mssg.To.Add(CorreoEnvia);
                                mssg.From = new MailAddress(_sender, "Documentos firmados", System.Text.Encoding.UTF8);
                                mssg.Bcc.Add(new MailAddress(_copiaCorreo));
                                mssg.Subject = titulo;
                                mssg.SubjectEncoding = System.Text.Encoding.UTF8;
                                mssg.BodyEncoding = System.Text.Encoding.UTF8;
                                mssg.IsBodyHtml = true;
                                mssg.AlternateViews.Add(htmlView);

                                // Adjuntar el archivo creado a cada correo
                                using (FileStream adjuntopdff = new FileStream(adjunto, FileMode.Open, FileAccess.Read))
                                {
                                    mssg.Attachments.Add(new Attachment(adjuntopdff, NombreDocumento));

                                    // Enviar el mensaje utilizando el objeto mssg
                                    envia(mssg);
                                } // El FileStream se cierra automáticamente aquí
                            } // El MailMessage se cierra automáticamente aquí


                            Directory.Delete(CarTempo, true);
                        }

                    }


                    

                }
                else
                {

                    CorreoEnvia = await BuscaCorreo(idDocumento);

                    // Convertir el documento Base64 a bytes y crear la carpeta temporal y el archivo adjunto
                    byte[] FirmadoBytes = Convert.FromBase64String(documento);
                    string CarTempo = CarpetaCrea(Path.Combine(Directory.GetCurrentDirectory(), "Docs", "Correo", seg.codNonce));
                    string adjunto = CreaArchivo(CarTempo, FirmadoBytes, NombreDocumento);
                    string titulo = "UCAB: Documentos firmados";
                    string bodyCorreo = File.ReadAllText(templatePath);
                    string mensaje_html = "<br><br> Reciba un cordial saludo,<br><br>Adjunto remitimos el documento firmado por la/las autoridades respectivas de la Universidad Cat&oacute;lica Andr&eacute;s Bello de acuerdo a su solicitud.<br><br> Este documento es firmado digitalmente, por lo tanto pierde su vigencia una vez que es impreso. En caso que otra persona requiera verificar la autenticidad del mismo podr&aacute; ingresar en el portal https://documentosenlinea.ucab.edu.ve/verificarfirma y al escanear el c&oacute;digo QR podr&aacute; visualizar los datos del mismo. <br></h4>";
                    bodyCorreo = bodyCorreo.Replace("#cuerpocorreo#", mensaje_html);
                    bodyCorreo = bodyCorreo.Replace("#PIEFIRMA#", Pie);
                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(bodyCorreo, Encoding.UTF8, MediaTypeNames.Text.Html);

                    // Crear un nuevo objeto MailMessage en cada iteración
                    using (MailMessage mssg = new MailMessage())
                    {
                        mssg.To.Add(CorreoEnvia);
                        mssg.From = new MailAddress(_sender, "UCAB: Documentos firmados", System.Text.Encoding.UTF8);
                        mssg.Bcc.Add(new MailAddress(_copiaCorreo));
                        mssg.Subject = titulo;
                        mssg.SubjectEncoding = System.Text.Encoding.UTF8;
                        mssg.BodyEncoding = System.Text.Encoding.UTF8;
                        mssg.IsBodyHtml = true;
                        mssg.AlternateViews.Add(htmlView);

                        // Adjuntar el archivo creado a cada correo
                        using (FileStream adjuntopdff = new FileStream(adjunto, FileMode.Open, FileAccess.Read))
                        {
                            mssg.Attachments.Add(new Attachment(adjuntopdff, NombreDocumento));

                            // Enviar el mensaje utilizando el objeto mssg
                            envia(mssg);
                        } // El FileStream se cierra automáticamente aquí
                    } // El MailMessage se cierra automáticamente aquí


                    Directory.Delete(CarTempo, true);

                }


            }
            catch (Exception ex)
            {
                l.ErrorLog(this, ex);
            }
        }

        private string CreaArchivo(string CarTempo, byte[] bytesPdf, string NombreArchivo)
        {
            try
            {
                if (VerifiCarpeta(CarTempo))
                {
                    using (FileStream stream = System.IO.File.Create(CarTempo + NombreArchivo))
                    {
                        stream.Write(bytesPdf, 0, bytesPdf.Length);
                        stream.Close();
                        stream.Dispose();
                    }
                }
                return CarTempo + NombreArchivo;
            }
            catch (Exception ex)
            {
                l.ErrorLog(this, ex);
                throw;
            }
        }

        public string CarpetaCrea(string ruta)
        {
            try
            {
                if (Directory.Exists(ruta))
                { return ruta; }
                else
                {
                    Directory.CreateDirectory(ruta);
                    return ruta;
                }
            }
            catch (Exception ex)
            {
                l.ErrorLog(this, ex);
                throw;
            }
        }

        public bool VerifiCarpeta(string ruta)
        {
            try
            {
                if (Directory.Exists(ruta))
                { return true; }
                else
                {
                    Directory.CreateDirectory(ruta);
                    return true;
                }
            }
            catch (Exception ex)
            {
                l.ErrorLog(this, ex);
                return false;
                throw;
            }
        }

        private async Task<string> BuscaCorreo(string idDocumento)
        {
            string correoEncontrado = "";

            using (var connection = new SqlConnection(Conexion.CadenaSQL))
            {
                await connection.OpenAsync();
                string subQuery = "SELECT CorreoNuevo FROM CorreoNuevoRegistra WHERE IdDocumento = @IdDocumento";

                using (var subCommand = new SqlCommand(subQuery, connection))
                {
                    subCommand.Parameters.AddWithValue("@IdDocumento", idDocumento);

                    using (var subReader = await subCommand.ExecuteReaderAsync())
                    {
                        if (await subReader.ReadAsync())
                        {
                            correoEncontrado = subReader.GetString(0);
                        }
                    }
                }
            }

            return correoEncontrado;
        }


    }
}
