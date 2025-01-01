using Matrix.Data;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Matrix.WebServices.Authentication;
using Matrix.WebServices.Services;
using Microsoft.AspNetCore.Mvc;
using Timer = Matrix.Data.Models.Timer;

namespace Matrix.WebServices.Controllers;

[Route("timer")]
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
    [Route("create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public IActionResult CreateTimer([FromBody] Timer timer, bool alsoStart = false)
    {
        ProgramState.PreviousState = ProgramState.State;
        ProgramState.State = MatrixState.Timer;
        ProgramState.Timer = new MatrixTimer(timer);
        ProgramState.UpdateNextTick = true;

        if (alsoStart)
        {
            ProgramState.Timer.Start();
        }
        
        return Ok("Success");
    }

    [HttpPost]
    [Route("start")]
    public IActionResult StartTimer()
    {
        CheckIfNullTimer();
        
        ProgramState.Timer?.Start();
        return Ok("Success");
    }

    [HttpPost]
    [Route("stop")]
    public IActionResult StopTimer()
    {
        CheckIfNullTimer();
        
        ProgramState.Timer?.Cancel();
        return Ok("Success");
    }

    [HttpPost]
    [Route("pause")]
    public IActionResult PauseTimer()
    {
        CheckIfNullTimer();
        
        ProgramState.Timer?.Pause();
        return Ok("Success");
    }

    [HttpPost]
    [Route("resume")]
    public IActionResult ResumeTimer()
    {
        CheckIfNullTimer();
        
        ProgramState.Timer?.Start(false);
        return Ok("Success");
    }

    [HttpGet]
    [Route("state")]
    public IActionResult GetTimerState()
    {
        CheckIfNullTimer();
        
        return Ok(ProgramState.Timer?.State.ToString());
    }

    private void CheckIfNullTimer()
    {
        if (ProgramState.Timer == null)
        {
            throw new TimerException(WebConstants.TimerNull);
        }
    }
}