namespace archiver.Dtos;

public record MeasurementOut(long Id, string SensorId, DateTimeOffset Timestamp, double Value);
