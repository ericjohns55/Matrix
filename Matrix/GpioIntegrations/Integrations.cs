using System.Device.Gpio;
using Matrix.Data.Utilities;

namespace Matrix.GpioIntegrations;

public class Integrations
{
    public IBuzzerSensor? BuzzerSensor { get; init; }

    public static Integrations SetupIntegrations(IConfiguration configuration, bool mockIntegration = false)
    {
        bool ambientSensorConfigured = configuration.GetValue(ConfigConstants.AmbientSensorEnabled, false);
        bool buzzWithTimer = configuration.GetValue(ConfigConstants.BuzzWithTimer, false);
        int buzzerPin = configuration.GetValue(ConfigConstants.BuzzerSensorPin, -1);

        if (mockIntegration)
        {
            return new Integrations(new FakeBuzzer(), true, ambientSensorConfigured);
        }

        GpioController controller = new GpioController();
        IBuzzerSensor? buzzerSensor = null;
        
        if (buzzerPin != -1)
        {
            buzzerSensor = new BuzzerSensor(controller, buzzerPin);
        }

        return new Integrations(buzzerSensor, buzzWithTimer, ambientSensorConfigured);
    }

    private Integrations(IBuzzerSensor? buzzerSensor, bool buzzWithTimer, bool ambientSensorConfigured)
    {
        BuzzerSensor = buzzerSensor;
        BuzzWithTimer = buzzWithTimer;
        AmbientSensorConfigured = ambientSensorConfigured;

        if (AmbientSensorConfigured)
        {
            AmbientSensorEnabled = true;
        }
    }

    public bool AmbientSensorEnabled { get; set; }

    public bool AmbientSensorConfigured { get; init; }

    public bool BuzzWithTimer { get; init; }
}