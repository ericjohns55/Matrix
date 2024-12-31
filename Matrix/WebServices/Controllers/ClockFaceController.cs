using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.WebServices.Authentication;
using Matrix.WebServices.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.WebServices.Controllers;

[Route("face")]
[ApiKeyAuthFilter]
public class ClockFaceController : Controller
{
    private readonly ILogger<ClockFaceController> _logger;
    private readonly IClockFaceService _clockFaceService;

    public ClockFaceController(ILogger<ClockFaceController> logger, IClockFaceService clockFaceService)
    {
        _logger = logger;
        _clockFaceService = clockFaceService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ClockFace>))]
    public IActionResult GetAllClockFaces()
    {
        return Ok(_clockFaceService.GetAllClockFaces());
    }

    [HttpGet("deleted")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ClockFace>))]
    public IActionResult GetDeletedClockFaces()
    {
        return Ok(_clockFaceService.GetAllClockFaces(SearchFilter.Deleted));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClockFace))]
    public IActionResult GetClockFace(int id)
    {
        return Ok(_clockFaceService.GetClockFace(id));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ClockFace))]
    public IActionResult AddClockFace([FromBody] ClockFace clockFace)
    {
        _logger.LogInformation($"Adding clock face: {clockFace.Name}");
        _clockFaceService.AddClockFace(clockFace);
        return Ok(clockFace);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClockFace))]
    public IActionResult UpdateClockFace(int id, [FromBody] ClockFace clockFace)
    {
        _logger.LogInformation($"Updating clock face with ID {id}");
        return Ok(_clockFaceService.UpdateClockFace(id, clockFace));
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]

    public IActionResult DeleteClockFace(int id)
    {
        _logger.LogInformation($"Deleting clock face with ID {id}");
        return Ok(_clockFaceService.RemoveClockFace(id));
    }
}