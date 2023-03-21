using RmqBenchmark.Kafka;
using RmqBenchmark.MassTransit;
using RmqBenchmark.NativeClient;

namespace RmqBenchmark;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly MtBenchmark _mtBenchmark;
    private readonly NcBenchmark _ncBenchmark;
    private readonly KfBenchmark _kfBenchmark;

    public Worker(ILogger<Worker> logger, MtBenchmark mtBenchmark, NcBenchmark ncBenchmark, KfBenchmark kfBenchmark)
    {
        _logger = logger;
        _mtBenchmark = mtBenchmark;
        _ncBenchmark = ncBenchmark;
        _kfBenchmark = kfBenchmark;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var benchmarkItems = new (string name, IBenchmark benchmark, int messageCount, bool consumeDeleayedStart)[] {
            ("MASSTRANSIT", _mtBenchmark, 200, false),
            ("NATIVE CLIENT", _ncBenchmark, 50000, false),
            ("KAFKA", _kfBenchmark, 50000, false)
            };
        foreach (var (name, benchmark, messageCount, consumeDeleayedStart) in benchmarkItems)
        {
            await benchmark.Run(name, messageCount, consumeDeleayedStart);
        }
        _logger.LogInformation("ALL BENCHMARKS FINISHED");
    }
}
