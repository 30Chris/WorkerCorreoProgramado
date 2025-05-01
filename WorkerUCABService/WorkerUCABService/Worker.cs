using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace WorkerUCABService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IFileData _fileData;
        private readonly Conexion _conexion;
        private readonly bool _startOnInstall;
        private readonly bool _ProcesoOn;

        public Worker(ILogger<Worker> logger, IFileData fileData, Conexion conexion, IConfiguration configuration)
        {
            _logger = logger;
            _fileData = fileData;
            _conexion = conexion;
            _startOnInstall = configuration.GetValue<bool>("WorkerConfig:StartOnInstall");
            _ProcesoOn = configuration.GetValue<bool>("WorkerConfig:ProcesoOn");
        }

       

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Comentar para correr la aplicacion en local

                //Detiene la funcionalidad del worker mientras se instala

                //if (!_startOnInstall)
                //{
                //    _logger.LogInformation("Worker not starting as per configuration during installation.");
                //    stoppingToken.ThrowIfCancellationRequested();
                //    return; // Sale del método de manera controlada sin terminar abruptamente.
                //}

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    string cadenaSql = Conexion.CadenaSQL;
                    _logger.LogInformation("Using connection string: {cadenaSql}", cadenaSql);

                    if (!_ProcesoOn)
                    {
                        await _fileData.QueryDatabase();
                        _logger.LogInformation("Proceso Registrado");
                    }
                    else
                    {
                        _logger.LogInformation("Servicio iniciado esperando indicaciones");
                    }

                    await Task.Delay(10000, stoppingToken); // Utiliza el token para cancelar la espera si es necesario.
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Worker execution canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the worker.");
            }
            finally
            {
                _logger.LogInformation("Worker has stopped.");
            }
        }

    }
}