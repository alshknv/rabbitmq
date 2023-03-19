using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static Newtonsoft.Json.JsonConvert;

namespace RmqBenchmark.NativeClient;

public class NcBenchmark : BaseBenchmark
{
    private const string QueueName = "SampleMessage";
    private const string ExchangeName = "ISampleMessage";
    private readonly INcConnection _connection;
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
        channel.QueueDeclare(QueueName, true, false, true, null);
        channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, true, true, null);
        channel.QueueBind(QueueName, ExchangeName, "");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (ch, ea) =>
        {
            var body = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
            ISampleMessage message = DeserializeObject<SampleMessage>(body)!;
            Task.Run(() =>
            {
                Consume(message);
                channel.BasicAck(ea.DeliveryTag, false);
            });
        };
        channel.BasicConsume(QueueName, false, consumer);
    }

    protected async override Task Publish(ISampleMessage message)
    {
        var channel = _connection.GetChannel();
        await Task.Run(() => channel.BasicPublish(ExchangeName, "", null, System.Text.Encoding.UTF8.GetBytes(SerializeObject(message))));
    }
}