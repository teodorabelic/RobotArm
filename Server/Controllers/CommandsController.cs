using Microsoft.AspNetCore.Mvc;
using Server.Domain;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("api/commands")]
public class CommandsController : ControllerBase
{
    private readonly PriorityQueues _queues;
    public CommandsController(PriorityQueues queues) { _queues = queues; }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CommandRequestDto dto)
    {
        var role = HttpContext.Items["Role"]?.ToString() ?? "NONE";
        var clientId = HttpContext.Items["ClientId"]?.ToString() ?? dto.ClientId;

        if (!Enum.TryParse<Command>(dto.Command, true, out var cmd))
            return BadRequest("Invalid command");

        var ec = new EnqueuedCommand(clientId, role, cmd);
        if (role == "K1") await _queues.High.Writer.WriteAsync(ec);
        else await _queues.Normal.Writer.WriteAsync(ec);

        return Accepted();
    }
}