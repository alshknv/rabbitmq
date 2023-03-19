namespace RmqBenchmark;

public interface ISampleMessage
{
    int Id { get; set; }
    DateTime? Created { get; set; }
    DateTime? Sent { get; set; }
    DateTime? Consumed { get; set; }
}

public class SampleMessage : ISampleMessage
{
    public int Id { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? Sent { get; set; }
    public DateTime? Consumed { get; set; }
    public static ISampleMessage Create(int id)
    {
        return new SampleMessage
        {
            Id = id
        };
    }
}