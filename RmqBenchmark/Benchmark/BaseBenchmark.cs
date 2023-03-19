using System.Collections.Concurrent;

namespace RmqBenchmark;

public interface IBenchmark
{
    bool IsRunning { get; }
    Task Run(string name, int count);
    void Consume(ISampleMessage message);
}

public abstract class BaseBenchmark : IBenchmark
{
    private readonly ILogger<BaseBenchmark> _logger;
    private readonly ConcurrentBag<ISampleMessage> SentMessagesBag = new();
    private readonly ConcurrentBag<ISampleMessage> ConsumedMessagesBag = new();

    private int MessagesTotal;
    private int PublishTasksCompleted => publishTasks.Count(t => t.IsCompleted);
    private bool PublishCodeFinished => MessagesTotal > 0 && PublishTasksCompleted == MessagesTotal;
    private int MessagesPublished => SentMessagesBag?.Count ?? 0;
    private bool PublishFinished => MessagesPublished == MessagesTotal;
    private int MessagesConsumed => ConsumedMessagesBag?.Count ?? 0;
    private bool ConsumeFinished => MessagesConsumed == MessagesTotal;
    private readonly ConcurrentBag<Task> publishTasks = new();

    private void InitBenchmark()
    {
        for (int i = 0; i < MessagesTotal; i++)
        {
            publishTasks.Add(Task.Factory.StartNew(async () =>
            {
                ISampleMessage message = SampleMessage.Create(i);
                message.Created = DateTime.Now;
                await Publish(message);
                message.Sent = DateTime.Now;
                SentMessagesBag.Add(message);
            }));
        }
    }

    protected BaseBenchmark(ILogger<BaseBenchmark> logger)
    {
        _logger = logger;
    }

    public bool IsRunning => MessagesTotal > 0 && (!PublishFinished || !ConsumeFinished);

    protected abstract void PreInit();
    protected abstract Task Publish(ISampleMessage message);
    protected abstract void Close();

    public async Task Run(string name, int count)
    {
        if (IsRunning) throw new InvalidOperationException("MassTransit benchmark {name} is running");
        ConsumedMessagesBag.Clear();
        SentMessagesBag.Clear();
        MessagesTotal = count;
        PreInit();
        var publishThreadTask = Task.Run(() => InitBenchmark());
        _logger.LogInformation("{benchmarkName} BENCHMARK:", name);
        while (IsRunning)
        {
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
            };
        if (PublishTasksCompleted > 0)
        {
            status.Add($"Publishing code has {(PublishCodeFinished ? "finished" : "not finished")} ({PublishTasksCompleted} / {MessagesTotal}) {Environment.NewLine}");
        }
        status.Add($"Publishing has {(PublishFinished ? "finished" : "not finished yet")} ({SentMessagesBag.Count} / {MessagesTotal}) {Environment.NewLine}");
        if (MessagesPublished > 0)
        {
            status.Add($"Average publish rate {MessagesPublished / ((DateTime)SentMessagesBag.Max(m => m.Sent)! - (DateTime)SentMessagesBag!.Min(m => m.Sent)!).TotalSeconds:0} / sec {Environment.NewLine}");
        }
        status.Add($"Consuming has {(ConsumeFinished ? "finished" : "not finished yet")} ({ConsumedMessagesBag.Count} / {MessagesTotal}) {Environment.NewLine}");
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
        summary.Add($"Publishing code started at {minTime}, finished at {maxTime}, took {(maxTime - minTime).TotalSeconds:0} sec {Environment.NewLine}");
        minTime = (DateTime)SentMessagesBag.Min(m => m.Sent)!;
        maxTime = (DateTime)SentMessagesBag.Max(m => m.Sent)!;
        summary.Add($"Real publishing to RABBITMQ by MT started at {minTime}, finished at {maxTime}, took {(maxTime - minTime).TotalSeconds:0} sec {Environment.NewLine}");
        minTime = (DateTime)ConsumedMessagesBag.Min(m => m.Consumed)!;
        maxTime = (DateTime)ConsumedMessagesBag.Max(m => m.Consumed)!;
        summary.Add($"Consuming from RABBITMQ by MT started at {minTime}, finished at {maxTime}, took {(maxTime - minTime).TotalSeconds:0} sec {Environment.NewLine}");
        summary.Add($"Total {MessagesTotal} messages were processed {Environment.NewLine}");
        return summary;
    }
}