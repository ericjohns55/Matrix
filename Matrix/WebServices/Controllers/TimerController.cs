using Matrix.Data;
using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.WebServices.Authentication;
using Matrix.WebServices.Services;
using Microsoft.AspNetCore.Mvc;
using Timer = Matrix.Data.Models.Timer;

namespace Matrix.WebServices.Controllers;

[Route("timers")]
[ApiKeyAuthFilter]
public class TimerController : Controller
{
    private readonly ILogger<TimerController> _logger;
    private readonly IMatrixService _matrixService;

    public TimerController(ILogger<TimerController> logger, IMatrixService matrixService)
    {
        _logger = logger;
        _matrixService = matrixService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public IActionResult StartTimer([FromBody] Timer timer)
    {
        ProgramState.PreviousState = ProgramState.State;
        ProgramState.State = MatrixState.Timer;
        ProgramState.Timer = new MatrixTimer(timer);
        ProgramState.UpdateNextTick = true;
        
        return Ok("Success");
    }

    [HttpPost]
    [Route("start")]
    public IActionResult StartTimer()
    {
        ProgramState.Timer?.Start();
        return Ok("Success");
    }

    [HttpPost]
    [Route("stop")]
    public IActionResult StopTimer()
    {
        ProgramState.Timer?.Cancel();
        return Ok("Success");
    }
}