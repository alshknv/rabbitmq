using System.Collections.Concurrent;

namespace RmqBenchmark;

public interface IBenchmark
{
    bool IsRunning { get; }
    Task Run(string name, int count, bool consumeDeleayedStart = false);
    void Consume(ISampleMessage message);
}

public abstract class BaseBenchmark : IBenchmark
{
    private readonly ILogger<BaseBenchmark> _logger;
    private readonly ConcurrentBag<ISampleMessage> SentMessagesBag = new();
    private readonly ConcurrentBag<ISampleMessage> ConsumedMessagesBag = new();
    private int MessagesTotal;
    private int MessagesConsumed => ConsumedMessagesBag?.Count ?? 0;
    private bool ConsumeFinished => MessagesConsumed >= MessagesTotal;

    private async Task PublishMessages()
    {
        var benchmarkPublishTasks = new List<Task>();
        for (int i = 0; i < MessagesTotal; i++)
        {
            ISampleMessage message = SampleMessage.Create(i);
            message.Created = DateTime.Now;
            benchmarkPublishTasks.Add(Publish(message));
            SentMessagesBag.Add(message);
        }
        await Task.WhenAll(benchmarkPublishTasks);
    }

    protected BaseBenchmark(ILogger<BaseBenchmark> logger)
    {
        _logger = logger;
    }

    public bool IsRunning => MessagesTotal > 0 && !ConsumeFinished;

    protected abstract void PreInit();
    protected abstract void BeginConsume();
    protected abstract Task Publish(ISampleMessage message);
    protected abstract void Close();

    public async Task Run(string name, int count, bool consumeDeleayedStart = false)
    {
        if (IsRunning) throw new InvalidOperationException("MassTransit benchmark {name} is running");
        ConsumedMessagesBag.Clear();
        SentMessagesBag.Clear();
        MessagesTotal = count;
        _logger.LogInformation("{benchmarkName} BENCHMARK:", name);
        PreInit();
        if (!consumeDeleayedStart)
        {
            BeginConsume();
        }
        _logger.LogInformation("{benchmarkName} publish {messagesTotal} messages:", name, MessagesTotal);
        await PublishMessages();
        while (IsRunning)
        {
            if (consumeDeleayedStart)
            {
                _logger.LogInformation("PUBLISHING HAS FINISHED, BEGIN CONSUMING...");
                BeginConsume();
                consumeDeleayedStart = false;
            }
            _logger.LogInformation("{status}", GetStatus());
            await Task.Delay(3000);
        }
        _logger.LogInformation("{status}", GetStatus());
        _logger.LogInformation("{summary}", GetSummary());
        Close();
    }

    public void Consume(ISampleMessage message)
    {
        message.Consumed = DateTime.Now;
        ConsumedMessagesBag.Add(message);
    }

    public List<string> GetStatus()
    {
        var status = new List<string>
        {
            $"Benchmark is {(IsRunning ? "running" : "not running")} {Environment.NewLine}",
            $"Consuming has {(ConsumeFinished ? "finished" : "not finished yet")} ({ConsumedMessagesBag.Count} / {MessagesTotal}) {Environment.NewLine}"
        };
        if (MessagesConsumed > 0)
        {
            status.Add($"Average consuming rate {MessagesConsumed / ((DateTime)ConsumedMessagesBag.Max(m => m.Consumed)! - (DateTime)ConsumedMessagesBag.Min(m => m.Consumed)!).TotalSeconds:0} / sec {Environment.NewLine}");
        }
        return status;
    }

    public List<string> GetSummary()
    {
        var summary = new List<string>
        {
            $"{Environment.NewLine}",
            $"SUMMARY: {Environment.NewLine}"
        };
        var minTime = (DateTime)SentMessagesBag.Min(m => m.Created)!;
        var maxTime = (DateTime)SentMessagesBag.Max(m => m.Created)!;
        summary.Add($"Publishing started at {minTime}, finished at {maxTime}, took {(maxTime - minTime).TotalSeconds:0} sec {Environment.NewLine}");
        minTime = (DateTime)ConsumedMessagesBag.Min(m => m.Consumed)!;
        maxTime = (DateTime)ConsumedMessagesBag.Max(m => m.Consumed)!;
        summary.Add($"Consuming from RABBITMQ by MT started at {minTime}, finished at {maxTime}, took {(maxTime - minTime).TotalSeconds:0} sec {Environment.NewLine}");
        summary.Add($"Total {MessagesTotal} messages were processed {Environment.NewLine}");
        return summary;
    }
}