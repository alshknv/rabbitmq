namespace RmqBenchmark;

public class BusConfig
{
    public string Host { get; set; } = default!;
    public ushort Port { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}