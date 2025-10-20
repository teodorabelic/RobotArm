using Microsoft.AspNetCore.Mvc;
using Server.Domain;

namespace Server.Controllers;

[ApiController]
[Route("api/state")]
public class StateController : ControllerBase
{
    private readonly ArmState _state;
    public StateController(ArmState state) { _state = state; }

    [HttpGet]
    public ActionResult<StateDto> Get() => new StateDto(_state.X, _state.Y, _state.Rot);
}