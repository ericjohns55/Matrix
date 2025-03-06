using Matrix.Data;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Matrix.Display;
using Matrix.WebServices.Authentication;
using Matrix.WebServices.Services;
using Microsoft.AspNetCore.Mvc;
using Timer = Matrix.Data.Models.Timer;

namespace Matrix.WebServices.Controllers;

[Route("timer")]
[ApiKeyAuthFilter]
public class TimerController : MatrixBaseController
{
    private readonly ILogger<TimerController> _logger;
    private readonly ClockFaceService _clockFaceService;

    public TimerController(ILogger<TimerController> logger, ClockFaceService clockFaceService)
    {
        _logger = logger;
        _clockFaceService = clockFaceService;
    }

    [HttpGet]
    [Route("current")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<MatrixTimer?>))]
    public IActionResult GetCurrentTimer()
    {
        return Ok(ExecuteToMatrixResponse(() => ProgramState.Timer));
    }

    [HttpPost]
    [Route("create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<MatrixTimer>))]
    public async Task<IActionResult> CreateTimer([FromBody] Timer timer, bool alsoStart = false)
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
        {
            var timerFace = await _clockFaceService.GetTimerClockFace(timer.TimerFaceId);
            MatrixMain.MatrixUpdater.UpdateTimerFace(timerFace);

            if (ProgramState.State != MatrixState.Timer)
            {
                ProgramState.PreviousState = ProgramState.State;
                ProgramState.State = MatrixState.Timer;
            }
            
            ProgramState.Timer = new MatrixTimer(timer);
            ProgramState.UpdateNextTick = true;

            if (alsoStart)
            {
                ProgramState.Timer.Start();
            }

            return ProgramState.Timer;
        }));
    }

    [HttpPost]
    [Route("start")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<MatrixTimer>))]
    public IActionResult StartTimer()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            CheckIfNullTimer();

            ProgramState.Timer?.Start();

            return ProgramState.Timer;
        }));
    }

    [HttpPost]
    [Route("stop")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<MatrixTimer>))]
    public IActionResult StopTimer()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            CheckIfNullTimer();

            ProgramState.Timer?.Cancel();

            return ProgramState.Timer;
        }));
    }

    [HttpPost]
    [Route("pause")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<MatrixTimer>))]
    public IActionResult PauseTimer()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            CheckIfNullTimer();

            ProgramState.Timer?.Pause();

            return ProgramState.Timer;
        }));
    }

    [HttpPost]
    [Route("resume")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<MatrixTimer>))]
    public IActionResult ResumeTimer()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            CheckIfNullTimer();

            ProgramState.Timer?.Start(false);

            return ProgramState.Timer;
        }));
    }

    [HttpGet]
    [Route("state")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<MatrixState>))]
    public IActionResult GetTimerState()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            CheckIfNullTimer();

            return ProgramState.Timer?.State;
        }));
    }

    [HttpPost]
    [Route("modify")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<MatrixTimer>))]
    public IActionResult ModifyTimer([FromBody] TimerModification payload)
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            CheckIfNullTimer();

            if (ValidateModification(ProgramState.Timer!, payload).Count == 0)
            {
                ProgramState.Timer?.Modify(payload);
            }
            
            return ProgramState.Timer;
        }));
    }

    [HttpPost]
    [Route("reset")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<MatrixTimer>))]
    public IActionResult ResetStopwatch()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            CheckIfNullTimer();

            ProgramState.Timer?.Reset();

            return ProgramState.Timer;
        }));
    }
    
    [HttpPost]
    [Route("render")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<string>))]
    public async Task<IActionResult> RenderTimer([FromBody] Timer timer, bool trimHeader = false, int scaleFactor = 1)
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
        {
            var timerFace = await _clockFaceService.GetTimerClockFace(timer.TimerFaceId);

            if (timerFace == null)
            {
                throw new ClockFaceException(WebConstants.ClockFaceNull);
            }

            var rendering = MatrixRenderer.RenderTimer(timer, timerFace, scaleFactor);
            return MatrixRenderer.ImageToBase64(rendering, trimHeader);
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

    private List<string> ValidateModification(MatrixTimer currentTimer, TimerModification payload)
    {
        var invalidFields = new List<string>();
        if (currentTimer.Hour - payload.HourAmount <= 0)
        {
            invalidFields.Add("Hour");
        }

        if (currentTimer.Minute - payload.MinuteAmount <= 0)
        {
            invalidFields.Add("Minute");
        }

        if (currentTimer.Second - payload.SecondAmount <= 0)
        {
            invalidFields.Add("Second");
        }

        return invalidFields;
    }
}