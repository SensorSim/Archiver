namespace archiver.Models;

public class Measurement
{
    public long Id { get; set; }
    public string SensorId { get; set; } = default!;
    public DateTimeOffset Timestamp { get; set; }
    public double Value { get; set; }
}
