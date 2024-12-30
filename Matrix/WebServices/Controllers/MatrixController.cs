using Matrix.Data;
using Microsoft.AspNetCore.Mvc;
using Matrix.WebServices.Services;
using Matrix.Data.Models;
using Matrix.Utilities;

namespace Matrix.WebServices.Controllers;

[Route("matrix")]
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public IActionResult SendUpdate()
    {
        ProgramState.UpdateNextTick = !ProgramState.UpdateNextTick;
        return Ok(ProgramState.UpdateNextTick);
    }

    [HttpGet("test")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public IActionResult GetTestData()
    {
        // _logger.LogInformation("GetTestData");
        // return Ok(_matrixService.GetTestData());
        return Ok();
    }

    [HttpPost("test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult PostTestData([FromBody] TestData testData)
    {
        // _matrixService.AddTestDataAsync(testData);
        // return Ok(testData);
        return Ok();
    }

    [HttpGet("list")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TestData>))]
    public IActionResult GetTestDataList()
    {
        return Ok();
        // return Ok(_matrixService.GetTestDataList());
    }
}