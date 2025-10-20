using Microsoft.EntityFrameworkCore;

namespace Server.Data;

public class RobotArmDb : DbContext
{
    public RobotArmDb(DbContextOptions<RobotArmDb> options) : base(options) {}
    public DbSet<OperationLog> OperationLogs => Set<OperationLog>();
}

public class OperationLog
{
    public int Id { get; set; }
    public string ClientId { get; set; } = "";
    public string Command { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public bool Allowed { get; set; }
    public string? Reason { get; set; }
    public int FromX { get; set; }
    public int FromY { get; set; }
    public int FromRot { get; set; }
    public int ToX { get; set; }
    public int ToY { get; set; }
    public int ToRot { get; set; }
}