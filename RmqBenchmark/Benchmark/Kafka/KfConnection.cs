using Confluent.Kafka;

namespace RmqBenchmark.Kafka;

public interface IKfConnection
{
    IProducer<Null, string> GetProducer();
    IConsumer<Ignore, string> GetConsumer();
    void Close();
}

public class KfConnection : IKfConnection
{
    private IProducer<Null, string>? producer;
    private readonly ProducerConfig producerConfig;
    IConsumer<Ignore, string>? consumer;
    private readonly ConsumerConfig consumerConfig;
    public KfConnection()
    {
        producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
        };

        consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "foo",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
    }

    public IProducer<Null, string> GetProducer()
    {
        producer ??= new ProducerBuilder<Null, string>(producerConfig).Build();
        return producer;
    }

    public IConsumer<Ignore, string> GetConsumer()
    {
        consumer ??= new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        return consumer;
    }

    public void Close()
    {
        consumer?.Close();
        consumer?.Dispose();
        producer?.Dispose();
    }
}