using MassTransit;

namespace RmqBenchmark.MassTransit
{
    public class MtBenchmark : BaseBenchmark
    {
        private readonly IBusControl _bus;

        public MtBenchmark(IBusControl publishEndpoint)
        {
            _bus = publishEndpoint;
        }

        public override void PreInit()
        {
        }

        public override void Close()
        {
        }

        public async override Task Publish(ISampleMessage message)
        {
            await _bus.Publish(message);
        }
    }
}