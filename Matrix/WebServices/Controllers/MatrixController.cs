using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Matrix.WebServices.Services;
using Matrix.Data.Models;

namespace Matrix.WebServices.Controllers;

[Route("matrix")]
public class MatrixController  : Controller
{
    private readonly ILogger<MatrixController> _logger;
    private readonly IMatrixService _matrixService;

    public MatrixController(ILogger<MatrixController> logger, IMatrixService matrixService)
    {
        _logger = logger;
        _matrixService = matrixService;
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