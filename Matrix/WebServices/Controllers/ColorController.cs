using Matrix.Data.Models;
using Matrix.WebServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.WebServices.Controllers;

[Route("colors")]
public class ColorController : Controller
{
    private readonly ILogger<ColorController> _logger;
    private readonly IClockFaceService _clockFaceService;

    public ColorController(ILogger<ColorController> logger, IClockFaceService clockFaceService)
    {
        _logger = logger;
        _clockFaceService = clockFaceService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MatrixColor>))]
    public IActionResult GetColors()
    {
        return Ok(_clockFaceService.GetMatrixColors());
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixColor))]
    public IActionResult GetColor(int id)
    {
        return Ok(_clockFaceService.GetMatrixColor(id));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MatrixColor))]
    public IActionResult Post([FromBody] MatrixColor color)
    {
        _clockFaceService.AddMatrixColor(color);
        return Ok();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    public IActionResult Delete(int id)
    {
        return Ok(_clockFaceService.RemoveMatrixColor(id));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixColor))]
    public IActionResult Put(int id, [FromBody] MatrixColor color)
    {
        return Ok(_clockFaceService.UpdateMatrixColor(id, color));
    }
}