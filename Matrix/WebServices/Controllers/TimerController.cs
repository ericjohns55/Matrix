using Matrix.Data;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Matrix.WebServices.Authentication;
using Microsoft.AspNetCore.Mvc;
using Timer = Matrix.Data.Models.Timer;

namespace Matrix.WebServices.Controllers;

[Route("timer")]
[ApiKeyAuthFilter]
public class TimerController : MatrixBaseController
{
    private readonly ILogger<TimerController> _logger;

    public TimerController(ILogger<TimerController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [Route("create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<bool>))]
    public IActionResult CreateTimer([FromBody] Timer timer, bool alsoStart = false)
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            ProgramState.PreviousState = ProgramState.State;
            ProgramState.State = MatrixState.Timer;
            ProgramState.Timer = new MatrixTimer(timer);
            ProgramState.UpdateNextTick = true;

            if (alsoStart)
            {
                ProgramState.Timer.Start();
            }

            return true;
        }));
    }

    [HttpPost]
    [Route("start")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<bool>))]
    public IActionResult StartTimer()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            CheckIfNullTimer();

            ProgramState.Timer?.Start();

            return true;
        }));
    }

    [HttpPost]
    [Route("stop")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<bool>))]
    public IActionResult StopTimer()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            CheckIfNullTimer();

            ProgramState.Timer?.Cancel();
            
            return true;
        }));
    }

    [HttpPost]
    [Route("pause")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<bool>))]
    public IActionResult PauseTimer()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            CheckIfNullTimer();

            ProgramState.Timer?.Pause();
            
            return true;
        }));
    }

    [HttpPost]
    [Route("resume")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<bool>))]
    public IActionResult ResumeTimer()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            CheckIfNullTimer();

            ProgramState.Timer?.Start(false);

            return true;
        }));
    }

    [HttpGet]
    [Route("state")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<string>))]
    public IActionResult GetTimerState()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            CheckIfNullTimer();

            return ProgramState.Timer?.State.ToString() ?? "Unknown";
        }));
    }

    private void CheckIfNullTimer()
    {
        if (ProgramState.Timer == null)
        {
            _logger.LogInformation("Timer is null");
            throw new TimerException(WebConstants.TimerNull);
        }
    }
}