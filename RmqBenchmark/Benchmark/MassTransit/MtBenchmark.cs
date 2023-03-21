using MassTransit;

namespace RmqBenchmark.MassTransit
{
    public class MtBenchmark : BaseBenchmark
    {
        private readonly IBusControl _bus;

        public MtBenchmark(IBusControl publishEndpoint, ILogger<MtBenchmark> logger) : base(logger)
        {
            _bus = publishEndpoint;
        }

        protected override void PreInit()
        {
        }

        protected override void Close()
        {
        }

        protected override Task Publish(ISampleMessage message)
        {
            return _bus.Publish(message);
        }

        protected override void BeginConsume()
        {
        }
    }
}