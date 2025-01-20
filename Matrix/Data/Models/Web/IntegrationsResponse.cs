namespace Matrix.Data.Models.Web;

public class IntegrationsResponse
{
    public bool BuzzerEnabled { get; init; }
    public int BuzzerPin { get; init; }
    public bool BuzzesWithTimer { get; init; }

    public bool AmbientSensorSetup { get; init; }
    public bool AmbientSensorEnabled { get; init; }
}