using RmqBenchmark;
using RmqBenchmark.MassTransit;
using RmqBenchmark.NativeClient;
using MassTransit;

var config = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json").Build();

var busConfig = config.GetSection("BusConfig").Get<BusConfig>();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(busConfig);
        services.AddMassTransit(x =>
        {
            x.AddConsumer<MtConsumer>();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(busConfig.Host, busConfig.Port, "/", h =>
                {
                    h.Username(busConfig.Username);
                    h.Password(busConfig.Password);
                });
                cfg.ConfigureEndpoints(context);
            });
        });
        services.AddSingleton<MtBenchmark>();
        services.AddSingleton<INcConnection, NcConnection>();
        services.AddSingleton<NcBenchmark>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
