using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Server.Auth;
using Server.Data;
using Server.Domain;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1) Rešavanje apsolutne putanje za SQLite (iz appsettings.json) ---
var raw = builder.Configuration.GetConnectionString("RobotArm") ?? "Data Source=robotarm.db";
var csbIn = new SqliteConnectionStringBuilder(raw);

var contentRoot = builder.Environment.ContentRootPath; // npr. ...\Server\
var dataSource = csbIn.DataSource;
if (!Path.IsPathRooted(dataSource))
    dataSource = Path.GetFullPath(Path.Combine(contentRoot, dataSource));

// kreiraj folder ako ne postoji
var dir = Path.GetDirectoryName(dataSource);
if (!string.IsNullOrEmpty(dir))
    Directory.CreateDirectory(dir);

// finalni conn string (stabilan)
var sqliteCnn = new SqliteConnectionStringBuilder
{
    DataSource = dataSource,
    Mode = SqliteOpenMode.ReadWriteCreate,
    Cache = SqliteCacheMode.Shared
}.ToString();

// --- 2) EF Core registracija (Context + Factory na ISTI conn string) ---
builder.Services.AddDbContext<RobotArmDb>(opt => opt.UseSqlite(sqliteCnn));
builder.Services.AddDbContextFactory<RobotArmDb>(opt => opt.UseSqlite(sqliteCnn));

// --- 3) ClientsConfig: takođe stabilizuj putanju ---
var clientsRaw = builder.Configuration.GetValue<string>("ClientsConfigPath") ?? "clients.json";
var clientsPath = Path.IsPathRooted(clientsRaw)
    ? clientsRaw
    : Path.GetFullPath(Path.Combine(contentRoot, clientsRaw));
builder.Services.AddSingleton(new ClientsConfig(clientsPath));

// --- 4) Domen + Hosted servis ---
builder.Services.AddSingleton<ArmState>();
builder.Services.AddSingleton<PriorityQueues>();
builder.Services.AddHostedService<PriorityWorker>();

// --- 5) Middleware + API ---
builder.Services.AddTransient<ApiKeyMiddleware>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ako nemaš podešen HTTPS lokalno, privremeno isključi liniju ispod:
// app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api"),
    a => a.UseMiddleware<ApiKeyMiddleware>());

app.MapControllers();

// --- 6) MIGRIRAJ PRE starta workera (bitno za "no such table") ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RobotArmDb>();
    await db.Database.MigrateAsync();
}

// (opciono) ispiši koju DB fajl koristi runtime:
Console.WriteLine($"[DB] Using SQLite at: {dataSource}");

app.Run();
