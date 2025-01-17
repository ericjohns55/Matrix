using System.Device.Gpio;
using Matrix.Data.Utilities;

namespace Matrix.GpioIntegrations;

public class Integrations
{
    public BuzzerSensor? BuzzerSensor { get; init; }

    public static Integrations SetupIntegrations(IConfiguration configuration)
    {
        bool ambientSensorEnabled = configuration.GetValue(ConfigConstants.AmbientSensorEnabled, false);
        bool buzzWithTimer = configuration.GetValue(ConfigConstants.BuzzWithTimer, false);
        int buzzerPin = configuration.GetValue(ConfigConstants.BuzzerSensorPin, -1);

        GpioController controller = new GpioController();
        BuzzerSensor? buzzerSensor = null;
        
        if (buzzerPin != -1)
        {
            buzzerSensor = new BuzzerSensor(controller, buzzerPin);
        }

        return new Integrations(buzzerSensor, buzzWithTimer, ambientSensorEnabled);
    }

    private Integrations(BuzzerSensor? buzzerSensor, bool buzzWithTimer, bool ambientSensorEnabled)
    {
        BuzzerSensor = buzzerSensor;
        BuzzWithTimer = buzzWithTimer;
        AmbientSensorEnabled = ambientSensorEnabled;
    }

    public bool AmbientSensorEnabled { get; init; }

    public bool BuzzWithTimer { get; init; }
}