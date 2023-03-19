using MassTransit;

namespace RmqBenchmark.MassTransit;

public class MtConsumer : IConsumer<ISampleMessage>
{
    private readonly IBenchmark _benchmark;

    public MtConsumer(MtBenchmark benchmark)
    {
        _benchmark = benchmark;
    }

    public Task Consume(ConsumeContext<ISampleMessage> context)
    {
        return Task.Run(() => _benchmark.Consume(context.Message));
    }
}
