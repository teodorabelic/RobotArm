namespace Server.Domain;

public sealed class ArmState
{
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int Rot { get; set; } = 0; // 0,90,180,270

    public (bool ok, string reason, ArmState from, ArmState to) TryApply(Command cmd)
    {
        var from = new ArmState { X = X, Y = Y, Rot = Rot };
        var nx = X; var ny = Y; var nr = Rot;

        switch (cmd)
        {
            case Command.Left:  nx = X - 1; break;
            case Command.Right: nx = X + 1; break;
            case Command.Up:    ny = Y - 1; break;
            case Command.Down:  ny = Y + 1; break;
            case Command.Rotate: nr = (Rot + 90) % 360; break;
            default: return (false, "Unknown command", from, this);
        }

        if (nx is < 0 or > 4 || ny is < 0 or > 4)
            return (false, "Out of bounds (5x5)", from, this);

        X = nx; Y = ny; Rot = nr;
        var to = new ArmState { X = X, Y = Y, Rot = Rot };
        return (true, "", from, to);
    }
}

public enum Command { Left, Right, Up, Down, Rotate }

public record CommandRequestDto(string ClientId, string Command);
public record StateDto(int X, int Y, int Rot);