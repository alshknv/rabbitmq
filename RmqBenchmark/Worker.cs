using RmqBenchmark.MassTransit;
using RmqBenchmark.NativeClient;

namespace RmqBenchmark;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly MtBenchmark _mtBenchmark;
    private readonly NcBenchmark _ncBenchmark;

    public Worker(ILogger<Worker> logger, MtBenchmark mtBenchmark, NcBenchmark ncBenchmark)
    {
        _logger = logger;
        _mtBenchmark = mtBenchmark;
        _ncBenchmark = ncBenchmark;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var benchmarkItems = new (string name, IBenchmark benchmark, int messageCount)[] {
            ("MASSTRANSIT", _mtBenchmark, 200),
            ("NATIVE CLIENT", _ncBenchmark, 20000) };
        foreach (var (name, benchmark, messageCount) in benchmarkItems)
        {
            await benchmark.Run(name, messageCount);
        }
        _logger.LogInformation("ALL BENCHMARKS FINISHED");
    }
}
