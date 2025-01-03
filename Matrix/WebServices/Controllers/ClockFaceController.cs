using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Matrix.WebServices.Authentication;
using Matrix.WebServices.Services;
using Microsoft.AspNetCore.Http.HttpResults;
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

    [HttpPost("at")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<int>))]
    public async Task<IActionResult> GetClockFaceForTime([FromBody] TimePayload timePayload)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() =>
            _clockFaceService.GetClockFaceForTime(timePayload)));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixListResponse<ClockFace>))]
    public async Task<IActionResult> GetAllClockFaces()
    {
        return Ok(await ExecuteToMatrixListResponseAsync(() =>
            _clockFaceService.GetAllClockFaces()));
    }

    [HttpGet("deleted")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixListResponse<ClockFace>))]
    public async Task<IActionResult> GetDeletedClockFaces()
    {
        return Ok(await ExecuteToMatrixListResponseAsync(() =>
            _clockFaceService.GetAllClockFaces(SearchFilter.Deleted)));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<ClockFace>))]
    public async Task<IActionResult> GetClockFace(int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => 
            _clockFaceService.GetClockFace(id)));
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
}