using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static Newtonsoft.Json.JsonConvert;

namespace RmqBenchmark.NativeClient;

public class NcConsumer : DefaultBasicConsumer
{
    private readonly IModel _channel;
    private readonly IBenchmark _benchmark;
    public NcConsumer(IModel channel, IBenchmark benchmark)
    {
        _channel = channel;
        _benchmark = benchmark;
    }
    public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
    {
        var serializedMessage = System.Text.Encoding.UTF8.GetString(body.ToArray());
        ISampleMessage message = DeserializeObject<SampleMessage>(serializedMessage)!;
        _benchmark.Consume(message);
        _channel.BasicAck(deliveryTag, false);
    }
}