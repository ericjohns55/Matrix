namespace Matrix.GpioIntegrations;

public interface IBuzzerSensor
{
    public void Buzz(bool onOff);
    public void EnsureOff();
    public bool Status();
    public int PinNumber();
}