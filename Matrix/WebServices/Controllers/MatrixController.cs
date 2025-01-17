using System.Runtime.CompilerServices;
using Matrix.Data;
using Microsoft.AspNetCore.Mvc;
using Matrix.WebServices.Services;
using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Display;
using Matrix.Utilities;
using Matrix.WebServices.Authentication;

namespace Matrix.WebServices.Controllers;

[Route("matrix")]
[ApiKeyAuthFilter]
public class MatrixController  : Controller
{
    private readonly ILogger<MatrixController> _logger;
    private readonly IMatrixService _matrixService;
    private readonly IConfiguration _configuration;

    public MatrixController(
        ILogger<MatrixController> logger,
        IMatrixService matrixService,
        IConfiguration configuration)
    {
        _logger = logger;
        _matrixService = matrixService;
        _configuration = configuration;
    }

    [HttpGet("variables")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, string>))]
    public IActionResult GetVariables()
    {
        return Ok(ProgramState.CurrentVariables);
    }
    
    [HttpPost("variables")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, string>))]
    public async Task<IActionResult> UpdateVariables()
    {
        return Ok(await _matrixService.UpdateVariables());
    }
    
    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, object?>))]
    public IActionResult GetConfig()
    {
        return Ok(ConfigUtility.GetConfig(_configuration));
    }

    [HttpGet("state")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public IActionResult GetState()
    {
        return Ok(ProgramState.State.ToString());
    }

    [HttpGet("face")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClockFace))]
    public IActionResult GetCurrentClockFace()
    {
        return Ok(MatrixMain.MatrixUpdater.CurrentClockFace);
    }

    [HttpPost("update")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public IActionResult SendUpdate()
    {
        ProgramState.UpdateNextTick = !ProgramState.UpdateNextTick;
        return Ok(ProgramState.UpdateNextTick);
    }
    
    [HttpPost("brightness")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public IActionResult UpdateBrightness([FromBody] BrightnessPayload payload)
    {
        MatrixUpdater.MatrixBrightness = payload.Brightness;
        ProgramState.UpdateNextTick = true;
        
        Console.WriteLine($"Source: {payload.Source}");

        return Ok(true);
    }
}