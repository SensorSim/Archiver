using archiver.Data;
using archiver.Dtos;
using archiver.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
});

// Health checks (za Kubernetes)
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Ok(new { service = "archiver", status = "ok" }));

app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapHealthChecks("/health/ready");

// POST /measurements
app.MapPost("/measurements", async (MeasurementIn input, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.SensorId))
        return Results.BadRequest("sensorId is required");

    var m = new Measurement
    {
        SensorId = input.SensorId,
        Timestamp = input.Timestamp.ToUniversalTime(),
        Value = input.Value
    };

    db.Measurements.Add(m);
    await db.SaveChangesAsync();

    return Results.Created($"/measurements/{m.Id}",
        new MeasurementOut(m.Id, m.SensorId, m.Timestamp, m.Value));
});

// GET /measurements
app.MapGet("/measurements", async (
    string? sensorId,
    AppDbContext db) =>
{
    var q = db.Measurements.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(sensorId))
        q = q.Where(m => m.SensorId == sensorId);

    var data = await q
        .OrderByDescending(m => m.Timestamp)
        .Take(100)
        .Select(m => new MeasurementOut(m.Id, m.SensorId, m.Timestamp, m.Value))
        .ToListAsync();

    return Results.Ok(data);
});



for (var i = 0; i < 30; i++)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        break;
    }
    catch
    {
        Thread.Sleep(1000);
    }
}



app.Run();
