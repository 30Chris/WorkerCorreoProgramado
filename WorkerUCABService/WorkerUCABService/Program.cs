using WorkerUCABService;
using WorkerUCABService.Procesos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Hosting.WindowsServices;






IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
     .UseSerilog((context, services, configuration) =>
     {
         configuration
             .Enrich.FromLogContext()
             .WriteTo.Console()
             .WriteTo.File("Procesos/Plantillas/Logss/worker-log.txt", rollingInterval: RollingInterval.Day);
     })
    .ConfigureServices((context, services) =>
    {
        // Registrar IConfiguration para que pueda ser inyectada en otros servicios
        services.AddSingleton(context.Configuration);

        // Registrar los servicios necesarios
        services.AddSingleton<ServiciosUCAB>();  
        services.AddSingleton<Correo>();
        services.AddSingleton<IFileData, FileData>();
        services.AddSingleton<Conexion>();

        // Registrar IHostedService (Worker)
        services.AddHostedService<Worker>();

        // Configurar las opciones para SMTPSettings
        services.Configure<SMTPSettings>(context.Configuration.GetSection("SMTPSettings"));

        // Configurar las opciones para ServiciosUCAB
        services.Configure<ServiciosUCABSettings>(context.Configuration.GetSection("ServiciosUCAB"));

        // Configurar las opciones para DatosServicio
        services.Configure<DatosServicioUCAB>(context.Configuration.GetSection("DatosServicio"));

        // Configurar las opciones para ValidacionDocum
        services.Configure<ValidacionDocum>(context.Configuration.GetSection("ValidaDoc"));

    })
    .Build();

await host.RunAsync();

