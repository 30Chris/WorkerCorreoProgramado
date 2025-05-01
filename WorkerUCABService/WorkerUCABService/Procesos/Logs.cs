using System;
using System.IO;
using System.Web;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace WorkerUCABService.Procesos
{
    internal class Logs
    {

        public void ErrorLog(object Fuente, Exception error)
        {
            string rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), @"procesos/Plantillas/Logs");
            string archivoLog = Path.Combine(rutaCarpeta, "UCAB.txt");
            StackTrace stacktrace = new StackTrace();

            if (!Directory.Exists(rutaCarpeta))
            {
                Directory.CreateDirectory(rutaCarpeta);
            }

            using (StreamWriter sw = new StreamWriter(archivoLog, true))
            {
                sw.WriteLine("--> Error Presentado el " + DateTime.Today.ToString("dd-MM-yy ") + DateTime.Now.ToString("HH:mm:ss"));
                sw.WriteLine(Fuente.GetType().FullName);

                StackFrame? frame = stacktrace.GetFrame(1);
                if (frame != null)
                {
                    MethodBase? method = frame.GetMethod();
                    if (method != null)
                    {
                        sw.WriteLine(method.Name + " - " + error);
                    }
                    else
                    {
                        sw.WriteLine("Método desconocido - " + error);
                    }
                }
                else
                {
                    sw.WriteLine("StackFrame desconocido - " + error);
                }
            }
        }


        public void testDoc(string error)
        {
            string rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "Procesos", "Plantillas", "Logss");

            string ArchivoLog = rutaCarpeta + @"\UCAB.txt";

            if (Directory.Exists(rutaCarpeta))
            {
                if (File.Exists(ArchivoLog))
                {
                    StreamWriter sw = File.AppendText(ArchivoLog);
                    sw.WriteLine("--> Error Presentado el " + DateTime.Today.ToString("dd-MM-yy ") + DateTime.Now.ToString("HH:mm:ss") + " " + error);
                    sw.Close();
                }
                else
                {
                    StreamWriter sw = new StreamWriter(ArchivoLog);
                    sw.WriteLine("--> Error Presentado el " + DateTime.Today.ToString("dd-MM-yy ") + DateTime.Now.ToString("HH:mm:ss") + " " + error);
                    sw.Close();
                }
            }
            else
            {
                Directory.CreateDirectory(rutaCarpeta);
                StreamWriter sw = new StreamWriter(ArchivoLog);
                sw.WriteLine("--> Error Presentado el " + DateTime.Today.ToString("dd-MM-yy ") + DateTime.Now.ToString("HH:mm:ss") + " " + error);
                sw.Close();
            }
        }

    }
}
