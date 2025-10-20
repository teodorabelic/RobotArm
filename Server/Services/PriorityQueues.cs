using System.Threading.Channels;
using Server.Domain;

namespace Server.Services;

public record EnqueuedCommand(string ClientId, string Role, Command Command);

public class PriorityQueues
{
    public Channel<EnqueuedCommand> High = Channel.CreateUnbounded<EnqueuedCommand>();   // K1
    public Channel<EnqueuedCommand> Normal = Channel.CreateUnbounded<EnqueuedCommand>(); // K2,K3
}