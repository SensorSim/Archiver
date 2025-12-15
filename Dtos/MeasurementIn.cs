namespace archiver.Dtos;

public record MeasurementIn(string SensorId, DateTimeOffset Timestamp, double Value);
