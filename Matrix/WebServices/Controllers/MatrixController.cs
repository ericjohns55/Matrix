using System.Runtime.CompilerServices;
using Matrix.Data;
using Microsoft.AspNetCore.Mvc;
using Matrix.WebServices.Services;
using Matrix.Data.Models;
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
    
    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, object?>))]
    public IActionResult GetConfig()
    {
        return Ok(ConfigUtility.GetConfig(_configuration));
    }

    [HttpGet("face")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClockFace))]
    public IActionResult GetCurrentClockFace()
    {
        return Ok(MatrixMain.MatrixUpdater.ClockFace);
    }

    [HttpPost("update")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public IActionResult SendUpdate()
    {
        ProgramState.UpdateNextTick = !ProgramState.UpdateNextTick;
        return Ok(ProgramState.UpdateNextTick);
    }
}