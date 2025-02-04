using Matrix.Data.Exceptions;
using Matrix.Data.Models.Web;
using Matrix.WebServices.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.WebServices.Controllers;

[Route("integrations")]
[ApiKeyAuthFilter]
public class IntegrationsController : MatrixBaseController
{
    private readonly ILogger<IntegrationsController> _logger;

    public IntegrationsController(ILogger<IntegrationsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<IntegrationsResponse>))]
    public IActionResult GetIntegrations()
    {
        return Ok(ExecuteToMatrixResponse(() => new IntegrationsResponse()
        {
            BuzzerEnabled = MatrixMain.Integrations.BuzzerSensor != null,
            BuzzerPin = MatrixMain.Integrations.BuzzerSensor?.PinNumber() ?? -1,
            BuzzesWithTimer = MatrixMain.Integrations.BuzzWithTimer,
            AmbientSensorSetup = MatrixMain.Integrations.AmbientSensorConfigured,
            AmbientSensorEnabled = MatrixMain.Integrations.AmbientSensorEnabled
        }));
    }

    [HttpPost("buzzer")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<bool>))]
    public IActionResult ToggleBuzzer([FromBody] BuzzerPayload payload)
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            if (MatrixMain.Integrations.BuzzerSensor == null)
            {
                _logger.LogInformation("Attempt to toggle unconfigured buzzer failed");
                throw new IntegrationsException("Buzzer Sensor is not available");
            }

            MatrixMain.Integrations.BuzzerSensor?.Buzz(payload.On);
            
            bool status = MatrixMain.Integrations.BuzzerSensor!.Status();
            _logger.LogInformation($"Toggled buzzer: {(status ? "on" : "off")}");

            return status;
        }));
    }

    [HttpPost("ambient-sensor")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<bool>))]
    public IActionResult ToggleAmbientSensor([FromBody] SensorPayload payload)
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            if (!MatrixMain.Integrations.AmbientSensorConfigured)
            {
                throw new IntegrationsException("Ambient sensor is not set up");
            }

            MatrixMain.Integrations.AmbientSensorEnabled = payload.Enable;
            return MatrixMain.Integrations.AmbientSensorEnabled; 
        }));
    }
}