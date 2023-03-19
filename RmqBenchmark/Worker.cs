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
            _logger.LogInformation($"{name} BENCHMARK:");
            benchmark.PreInit();
            benchmark.Start(messageCount);
            while (benchmark.IsRunning && !stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("{status}", benchmark.GetStatus());
                await Task.Delay(3000);
            }
            _logger.LogInformation("{status}", benchmark.GetStatus());
            _logger.LogInformation("{summary}", benchmark.GetSummary());
            benchmark.Close();
        }
        _logger.LogInformation("ALL BENCHMARKS FINISHED");
    }
}
