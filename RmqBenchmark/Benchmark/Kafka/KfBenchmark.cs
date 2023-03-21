using Confluent.Kafka;
using static Newtonsoft.Json.JsonConvert;

namespace RmqBenchmark.Kafka;

public class KfBenchmark : BaseBenchmark
{
    private const string TopicName = "SampleMessage";
    private readonly IKfConnection _connection;
    private readonly ILogger<KfBenchmark> _logger;
    private CancellationTokenSource cancellationTokenSource;

    public KfBenchmark(IKfConnection connection, ILogger<KfBenchmark> logger) : base(logger)
    {
        _connection = connection;
        _logger = logger;
        cancellationTokenSource = new CancellationTokenSource();
    }

    protected override void BeginConsume()
    {
        var consumer = _connection.GetConsumer();
        consumer.Subscribe(TopicName);
        var consumeTask = Task.Run(() =>
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(cancellationTokenSource.Token);
                    ISampleMessage message = DeserializeObject<SampleMessage>(consumeResult.Message.Value)!;
                    Consume(message);
                    consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while consuming from Kafka");
                    break;
                }
            }
        });

    }

    protected override void Close()
    {
        cancellationTokenSource.Cancel();
        _connection.Close();
    }

    protected override void PreInit()
    {

    }

    protected override Task Publish(ISampleMessage message)
    {
        var producer = _connection.GetProducer();
        return producer.ProduceAsync(TopicName, new Message<Null, string> { Value = SerializeObject(message) });
    }
}