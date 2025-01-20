using Matrix.Data;
using Matrix.Data.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Matrix.WebServices.Services;
using Matrix.Data.Models.Web;
using Matrix.Data.Utilities;
using Matrix.Display;
using Matrix.Utilities;
using Matrix.WebServices.Authentication;

namespace Matrix.WebServices.Controllers;

[Route("matrix")]
[ApiKeyAuthFilter]
public class MatrixController  : MatrixBaseController
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
        return Ok(ExecuteToMatrixResponse(() => ProgramState.CurrentVariables));
    }
    
    [HttpPost("variables")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, string>))]
    public async Task<IActionResult> UpdateVariables()
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _matrixService.UpdateVariables()));
    }
    
    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, object?>))]
    public IActionResult GetConfig()
    {
        return Ok(ExecuteToMatrixResponse(() => ConfigUtility.GetConfig(_configuration)));
    }

    [HttpGet("overview")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<ProgramOverview>))]
    public IActionResult GetOverview()
    {
        return Ok(ExecuteToMatrixResponse(() => new ProgramOverview()
        {
            MatrixState = ProgramState.State,
            CurrentVariables = ProgramState.CurrentVariables,
            Timer = ProgramState.Timer,
            CurrentClockFaceForNow = MatrixMain.MatrixUpdater.CurrentClockFace,
            OverridenClockFace = ProgramState.OverrideClockFace ? MatrixUpdater.OverridenClockFace : null
        }));
    }

    [HttpPost("update")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<bool>))]
    public IActionResult SendUpdate()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            ProgramState.UpdateNextTick = true;
            return ProgramState.UpdateNextTick;
        }));
    }
    
    [HttpPost("brightness")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<bool>))]
    public IActionResult UpdateBrightness([FromBody] BrightnessPayload payload)
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            if (payload.Source == WebConstants.LightSensorSource && !MatrixMain.Integrations.AmbientSensorEnabled)
            {
                throw new BrightnessException(WebConstants.AmbientSensorDisabled);
            }
            
            _logger.LogInformation($"Updating brightness to {payload.Brightness}");
        
            MatrixUpdater.MatrixBrightness = payload.Brightness;
            ProgramState.UpdateNextTick = true;

            return true;
        }));
    }
}