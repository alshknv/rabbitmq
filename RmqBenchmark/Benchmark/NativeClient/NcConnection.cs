using RabbitMQ.Client;

namespace RmqBenchmark.NativeClient;

public interface INcConnection
{
    IModel GetChannel();
    void Close();
}

public class NcConnection : INcConnection
{
    private readonly ConnectionFactory factory = new();
    private IConnection? connection;
    private IModel? channel;
    public NcConnection(BusConfig busConfig)
    {
        factory.Uri = new Uri($"amqp://{busConfig.Username}:{busConfig.Password}@{busConfig.Host}:{busConfig.Port}/");
    }

    public IModel GetChannel()
    {
        connection ??= factory.CreateConnection();
        channel ??= connection.CreateModel();
        return channel;
    }

    public void Close()
    {
        channel?.Close();
        connection?.Close();
    }
}