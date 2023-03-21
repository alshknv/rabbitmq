using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static Newtonsoft.Json.JsonConvert;

namespace RmqBenchmark.NativeClient;

public class NcBenchmark : BaseBenchmark
{
    private const string QueueName = "SampleMessage";
    private const string ExchangeName = "ISampleMessage";
    private readonly INcConnection _connection;
    private IBasicProperties PublishProperties = default!;

    public NcBenchmark(INcConnection connection, ILogger<NcBenchmark> logger) : base(logger)
    {
        _connection = connection;
    }

    protected override void Close()
    {
        _connection.Close();
    }

    protected override void PreInit()
    {
        var channel = _connection.GetChannel();
        channel.QueueDeclare(QueueName, true, false, false, null);
        channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, true, false, null);
        channel.QueueBind(QueueName, ExchangeName, "");

        PublishProperties = channel.CreateBasicProperties();
        PublishProperties.Persistent = true;

        channel.BasicAcks += (a, b) =>
        {
        };
        //channel.ConfirmSelect();
    }

    protected override void BeginConsume()
    {
        var channel = _connection.GetChannel();
        channel.BasicConsume(QueueName, false, new NcConsumer(channel, this));
    }

    protected override Task Publish(ISampleMessage message)
    {
        var channel = _connection.GetChannel();
        channel.BasicPublish(ExchangeName, "", PublishProperties, System.Text.Encoding.UTF8.GetBytes(SerializeObject(message)));
        return Task.CompletedTask;
    }
}