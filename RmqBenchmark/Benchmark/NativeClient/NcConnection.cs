using RabbitMQ.Client;

namespace RmqBenchmark.NativeClient;

public interface INcConnection
{
    IModel GetChannel();
    void Close();
}

public class NcConnection : INcConnection
{
    private ConnectionFactory factory = new();
    private IConnection? connection;
    private IModel? channel;
    public NcConnection(BusConfig busConfig)
    {
        factory.Uri = new Uri($"amqp://{busConfig.Username}:{busConfig.Password}@{busConfig.Host}:{busConfig.Port}/");
    }

    public IModel GetChannel()
    {
        if (connection == null)
        {
            connection = factory.CreateConnection();
        }
        if (channel == null)
        {
            channel = connection.CreateModel();
        }
        return channel;
    }

    public void Close()
    {
        channel?.Close();
        connection?.Close();
    }
}