using Matrix.Data;
using Matrix.Data.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Matrix.WebServices.Services;
using Matrix.Data.Models.Web;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Matrix.Display;
using Matrix.Utilities;
using Matrix.WebServices.Authentication;
using Matrix.WebServices.Clients;

namespace Matrix.WebServices.Controllers;

[Route("matrix")]
[ApiKeyAuthFilter]
public class MatrixController  : MatrixBaseController
{
    private readonly ILogger<MatrixController> _logger;
    private readonly IConfiguration _configuration;

    public MatrixController(ILogger<MatrixController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok();
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
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
        {
            var weatherUrl = _configuration.GetValue<string>(ConfigConstants.WeatherUrl);

            if (weatherUrl != null)
            {
                using (var weatherClient = new WeatherClient(weatherUrl))
                {
                    ProgramState.Weather = await weatherClient.GetWeather();
                    ProgramState.UpdateVariables();
                    ProgramState.UpdateNextTick = true;
                }
            }

            return ProgramState.CurrentVariables;
        }));
    }
    
    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, object?>))]
    public IActionResult GetConfig()
    {
        return Ok(ExecuteToMatrixResponse(() => ConfigUtility.GetConfig(_configuration)));
    }

    private ProgramOverview GenerateProgramOverview() => new ProgramOverview()
    {
        MatrixState = ProgramState.State,
        MatrixInformation = new MatrixInformation()
        {
            Width = MatrixUpdater.MatrixWidth,
            Height = MatrixUpdater.MatrixHeight,
            Brightness = MatrixUpdater.MatrixBrightness
        },
        UpdateInterval = MatrixMain.MatrixUpdater.GetUpdateInterval(),
        CurrentVariables = ProgramState.CurrentVariables,
        Timer = ProgramState.Timer,
        PlainText = ProgramState.PlainText,
        ScrollingText = ProgramState.ScrollingText,
        CurrentClockFace = MatrixMain.MatrixUpdater.CurrentClockFace,
        OverridenClockFace = ProgramState.OverrideClockFace ? MatrixUpdater.OverridenClockFace : null
    };

    [HttpGet("overview")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<ProgramOverview>))]
    public IActionResult GetOverview()
    {
        return Ok(ExecuteToMatrixResponse(() => GenerateProgramOverview()));
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

    [HttpPost("restore")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<ProgramOverview>))]
    public IActionResult RestoreState()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            ProgramState.PreviousState = ProgramState.State;
            ProgramState.State = MatrixState.Clock;

            ProgramState.ScrollingText = null;
            ProgramState.PlainText = null;
            ProgramState.Timer = null;
            ProgramState.Image = null;

            ProgramState.OverrideClockFace = false;
            MatrixUpdater.OverridenClockFace = null;
            
            ProgramState.UpdateNextTick = true;
            return GenerateProgramOverview();
        }));
    }
    
    [HttpPost("brightness")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<bool>))]
    public IActionResult UpdateBrightness([FromBody] BrightnessPayload payload, bool disableAmbientSensor = false)
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            if (disableAmbientSensor && MatrixMain.Integrations.AmbientSensorEnabled)
            {
                MatrixMain.Integrations.AmbientSensorEnabled = false;
            }
            
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