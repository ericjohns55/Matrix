using Matrix.Data;
using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.WebServices.Services;
using Microsoft.AspNetCore.Mvc;
using Timer = Matrix.Data.Models.Timer;

namespace Matrix.WebServices.Controllers;

[Route("timers")]
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
        ProgramState.Timer = timer;
        ProgramState.Update = true;
        ProgramState.Timer.Start();
        
        return Ok("Success");
    }
}