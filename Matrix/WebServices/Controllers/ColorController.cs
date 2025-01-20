using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.WebServices.Authentication;
using Matrix.WebServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.WebServices.Controllers;

[Route("colors")]
[ApiKeyAuthFilter]
public class ColorController : MatrixBaseController
{
    private readonly ILogger<ColorController> _logger;
    private readonly IColorService _colorService;

    public ColorController(ILogger<ColorController> logger, IColorService colorService)
    {
        _logger = logger;
        _colorService = colorService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<List<MatrixColor>>))]
    public async Task<IActionResult> GetColors()
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _colorService.GetMatrixColors()));
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixColor))]
    public async Task<IActionResult> GetColor(int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _colorService.GetMatrixColor(id)));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MatrixColor))]
    public async Task<IActionResult> Post([FromBody] MatrixColor color)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() =>
        {
            _logger.LogInformation($"Creating new matrix color: {color.Name}");
            return _colorService.AddMatrixColor(color);
        }));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() =>
        {
            _logger.LogInformation($"Deleted matrix color with ID {id}");
            return _colorService.RemoveMatrixColor(id);
        }));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixColor))]
    public async Task<IActionResult> Put(int id, [FromBody] MatrixColor color)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() =>
        {
            _logger.LogInformation($"Updating matrix color with ID {id}");
            return _colorService.UpdateMatrixColor(id, color);
        }));
    }
}