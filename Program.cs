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
    DateTimeOffset? from,
    DateTimeOffset? to,
    int page,
    int pageSize,
    AppDbContext db) =>
{
    page = page <= 0 ? 1 : page;
    pageSize = pageSize <= 0 ? 100 : Math.Min(pageSize, 1000);

    var q = db.Measurements.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(sensorId))
        q = q.Where(m => m.SensorId == sensorId);

    if (from is not null)
        q = q.Where(m => m.Timestamp >= from.Value.UtcDateTime);

    if (to is not null)
        q = q.Where(m => m.Timestamp <= to.Value.UtcDateTime);

    var total = await q.CountAsync();

    var data = await q
        .OrderByDescending(m => m.Timestamp)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(m => new MeasurementOut(m.Id, m.SensorId, m.Timestamp, m.Value))
        .ToListAsync();

    return Results.Ok(new { page, pageSize, total, items = data });
});

// GET /measurements/{id}
app.MapGet("/measurements/{id:int}", async (int id, AppDbContext db) =>
{
    var m = await db.Measurements.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    if (m is null) return Results.NotFound();

    return Results.Ok(new MeasurementOut(m.Id, m.SensorId, m.Timestamp, m.Value));
});

// PUT /measurements/{id}
app.MapPut("/measurements/{id:int}", async (int id, MeasurementIn input, AppDbContext db) =>
{
    var m = await db.Measurements.FirstOrDefaultAsync(x => x.Id == id);
    if (m is null) return Results.NotFound();

    if (string.IsNullOrWhiteSpace(input.SensorId))
        return Results.BadRequest("sensorId is required");

    m.SensorId = input.SensorId;
    m.Timestamp = input.Timestamp.ToUniversalTime();
    m.Value = input.Value;

    await db.SaveChangesAsync();

    return Results.Ok(new MeasurementOut(m.Id, m.SensorId, m.Timestamp, m.Value));
});

// DELETE /measurements/{id}
app.MapDelete("/measurements/{id:int}", async (int id, AppDbContext db) =>
{
    var m = await db.Measurements.FirstOrDefaultAsync(x => x.Id == id);
    if (m is null) return Results.NotFound();

    db.Measurements.Remove(m);
    await db.SaveChangesAsync();
    return Results.NoContent();
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
