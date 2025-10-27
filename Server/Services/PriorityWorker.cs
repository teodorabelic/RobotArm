using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Data;
using Server.Domain;

namespace Server.Services;

public class PriorityWorker : BackgroundService
{
    private readonly PriorityQueues _queues;
    private readonly IDbContextFactory<RobotArmDb> _dbFactory;
    private readonly ArmState _state;
    private readonly ILogger<PriorityWorker> _log;

    public PriorityWorker(
        PriorityQueues queues,
        IDbContextFactory<RobotArmDb> dbFactory,
        ArmState state,
        ILogger<PriorityWorker> log)
    {
        _queues = queues;
        _dbFactory = dbFactory;
        _state = state;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("PriorityWorker started");
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_queues.High.Reader.TryRead(out var cmd) &&
                !_queues.Normal.Reader.TryRead(out cmd))
            {
                await Task.Delay(10, stoppingToken);
                continue;
            }

            var allowedByRole = cmd.Role switch
            {
                "K1" => true,
                "K2" => cmd.Command is Command.Left or Command.Right or Command.Up or Command.Down,
                "K3" => cmd.Command is Command.Rotate,
                _ => false
            };

            var now = DateTime.UtcNow;
            ArmState from, to;
            bool allowed;
            string reason;

            lock (_state) // ako je _state deljen kroz vise niti
            {
                from = new ArmState { X = _state.X, Y = _state.Y, Rot = _state.Rot };

                if (!allowedByRole)
                {
                    allowed = false; reason = "Forbidden by role";
                    to = new ArmState { X = _state.X, Y = _state.Y, Rot = _state.Rot };
                }
                else
                {
                    var (ok, why, f, t) = _state.TryApply(cmd.Command);
                    allowed = ok; reason = ok ? "" : why; from = f; to = t;
                    if (!ok)
                    {}
                }
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(stoppingToken);
                db.OperationLogs.Add(new OperationLog
                {
                    ClientId = cmd.ClientId,
                    Command = cmd.Command.ToString(),
                    Timestamp = now,
                    Allowed = allowed,
                    Reason = reason,
                    FromX = from.X,
                    FromY = from.Y,
                    FromRot = from.Rot,
                    ToX = to.X,
                    ToY = to.Y,
                    ToRot = to.Rot
                });
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to persist OperationLog for {ClientId}", cmd.ClientId);
            }
        }
        _log.LogInformation("PriorityWorker stopping");
    }
}
