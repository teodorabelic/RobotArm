using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Server.Auth;
using Server.Data;
using Server.Domain;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);
var raw = builder.Configuration.GetConnectionString("RobotArm") ?? "Data Source=robotarm.db";
var csbIn = new SqliteConnectionStringBuilder(raw);

var contentRoot = builder.Environment.ContentRootPath;
var dataSource = csbIn.DataSource;
if (!Path.IsPathRooted(dataSource))
    dataSource = Path.GetFullPath(Path.Combine(contentRoot, dataSource));

// kreiraj folder ako ne postoji
var dir = Path.GetDirectoryName(dataSource);
if (!string.IsNullOrEmpty(dir))
    Directory.CreateDirectory(dir);

// finalni conn string
var sqliteCnn = new SqliteConnectionStringBuilder
{
    DataSource = dataSource,
    Mode = SqliteOpenMode.ReadWriteCreate,
    Cache = SqliteCacheMode.Shared
}.ToString();

// builder.Services.AddDbContext<RobotArmDb>(opt => opt.UseSqlite(sqliteCnn));
builder.Services.AddDbContextFactory<RobotArmDb>(opt => opt.UseSqlite(sqliteCnn));

var clientsRaw = builder.Configuration.GetValue<string>("ClientsConfigPath") ?? "clients.json";
var clientsPath = Path.IsPathRooted(clientsRaw)
    ? clientsRaw
    : Path.GetFullPath(Path.Combine(contentRoot, clientsRaw));
builder.Services.AddSingleton(new ClientsConfig(clientsPath));

// hostovani servisi
builder.Services.AddSingleton<ArmState>();
builder.Services.AddSingleton<PriorityQueues>();
builder.Services.AddHostedService<PriorityWorker>();

// middleware i kontroleri
builder.Services.AddTransient<ApiKeyMiddleware>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ovde enablujem https
app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api"),
    a => a.UseMiddleware<ApiKeyMiddleware>());

app.MapControllers();

// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<RobotArmDb>();
//     await db.Database.MigrateAsync();
// }
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<RobotArmDb>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
}

Console.WriteLine($"[DB] Using SQLite at: {dataSource}");

app.Run();
