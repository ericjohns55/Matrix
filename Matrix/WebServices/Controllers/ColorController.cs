using Matrix.Data.Models;
using Matrix.WebServices.Authentication;
using Matrix.WebServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.WebServices.Controllers;

[Route("colors")]
[ApiKeyAuthFilter]
public class ColorController : Controller
{
    private readonly ILogger<ColorController> _logger;
    private readonly IColorService _colorService;

    public ColorController(ILogger<ColorController> logger, IColorService colorService)
    {
        _logger = logger;
        _colorService = colorService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MatrixColor>))]
    public IActionResult GetColors()
    {
        return Ok(_colorService.GetMatrixColors());
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixColor))]
    public IActionResult GetColor(int id)
    {
        return Ok(_colorService.GetMatrixColor(id));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MatrixColor))]
    public IActionResult Post([FromBody] MatrixColor color)
    {
        _logger.LogInformation($"Creating new matrix color: {color.Name}");
        _colorService.AddMatrixColor(color);
        return Ok(color);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    public IActionResult Delete(int id)
    {
        _logger.LogInformation($"Deleted matrix color with ID {id}");
        return Ok(_colorService.RemoveMatrixColor(id));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixColor))]
    public IActionResult Put(int id, [FromBody] MatrixColor color)
    {
        _logger.LogInformation($"Updating matrix color with ID {id}");
        return Ok(_colorService.UpdateMatrixColor(id, color));
    }
}