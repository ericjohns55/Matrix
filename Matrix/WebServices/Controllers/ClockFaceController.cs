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

namespace Matrix.WebServices.Controllers;

[Route("clockface")]
[ApiKeyAuthFilter]
public class ClockFaceController : MatrixBaseController
{
    private readonly ILogger<ClockFaceController> _logger;
    private readonly IClockFaceService _clockFaceService;

    public ClockFaceController(ILogger<ClockFaceController> logger, IClockFaceService clockFaceService)
    {
        _logger = logger;
        _clockFaceService = clockFaceService;
    }

    [HttpGet("timer/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<ClockFace>))]
    public async Task<IActionResult> GetTimerClockFace(int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() =>
            _clockFaceService.GetTimerClockFace(id)));
    }
    
    [HttpGet("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateTimePeriods()
    {
        return Ok(await ExecuteToMatrixResponseAsync(() =>
            _clockFaceService.ValidateClockFaceTimePeriods()));
    }

    [HttpPost("at")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<ClockFace>))]
    public async Task<IActionResult> GetClockFaceForTime([FromBody] TimePayload timePayload)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() =>
            _clockFaceService.GetClockFaceForTime(timePayload)));
    }

    [HttpPost("override/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<ClockFace>))]
    public async Task<IActionResult> OverrideClockFace(int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
        {
            MatrixUpdater.OverridenClockFace = await _clockFaceService.GetClockFace(id);

            ProgramState.OverrideClockFace = true;
            ProgramState.UpdateNextTick = true;

            return MatrixUpdater.OverridenClockFace;
        }));
    }
    
    [HttpPost("override")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<bool>))]
    public IActionResult CancelOverride()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            MatrixUpdater.OverridenClockFace = null;
            
            ProgramState.OverrideClockFace = false;
            ProgramState.UpdateNextTick = true;
            
            return true;
        }));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixListResponse<ClockFace>))]
    public async Task<IActionResult> GetAllClockFaces(bool timerFace = false, bool render = false, int scaleFactor = 1)
    {
        return Ok(await ExecuteToMatrixListResponseAsync(() =>
            _clockFaceService.GetAllClockFaces(SearchFilter.Active, timerFace, render, scaleFactor)));
    }

    [HttpGet("deleted")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixListResponse<ClockFace>))]
    public async Task<IActionResult> GetDeletedClockFaces(bool timerFace = false, bool render = false, int scaleFactor = 1)
    {
        return Ok(await ExecuteToMatrixListResponseAsync(() =>
            _clockFaceService.GetAllClockFaces(SearchFilter.Deleted, timerFace, render, scaleFactor)));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<ClockFace>))]
    public async Task<IActionResult> GetClockFace(int id, bool render = false, int scaleFactor = 1)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => 
            _clockFaceService.GetClockFace(id, render, scaleFactor)));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MatrixResponse<ClockFace>))]
    public async Task<IActionResult> AddClockFace([FromBody] ClockFace clockFace)
    {
        if (clockFace == null)
        {
            throw new ClockFaceException(WebConstants.ClockFaceNull);
        }
        
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
        {
            _logger.LogInformation($"Adding clock face: {clockFace.Name}");

            return await _clockFaceService.AddClockFace(clockFace);
        }));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<ClockFace>))]
    public async Task<IActionResult> UpdateClockFace(int id, [FromBody] ClockFace clockFace)
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
        {
            _logger.LogInformation($"Updating clock face with ID {id}");
            
            return await _clockFaceService.UpdateClockFace(id, clockFace);
        }));
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    public async Task<IActionResult> DeleteClockFace(int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
        {
            _logger.LogInformation($"Deleting clock face with ID {id}");
            return await _clockFaceService.RemoveClockFace(id);
        }));
    }

    [HttpPost("render")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<string>))]
    public IActionResult RenderClockFace([FromBody] ClockFace clockFace, bool trimHeader = false, int scaleFactor = 1, bool useCurrentVariables = true)
    {
        return Ok(ExecuteToMatrixResponse(() =>
            MatrixRenderer.ImageToBase64(MatrixRenderer.RenderClockFace(clockFace, scaleFactor, useCurrentVariables), trimHeader)));
    }
}