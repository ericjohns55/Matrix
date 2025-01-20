namespace Matrix.GpioIntegrations;

public class FakeBuzzer : IBuzzerSensor
{
    private bool _isOn;
    
    public FakeBuzzer()
    {
        _isOn = false;
    }

    public void Buzz(bool onOff)
    {
        _isOn = onOff;
        
        Console.WriteLine($"Buzzing {(_isOn ? "enabled" : "disabled")}");
    }

    public void EnsureOff()
    {
        if (_isOn)
        {
            Console.WriteLine("Forcing buzz off");
            Buzz(false);
        }
    }
    
    public bool Status() => _isOn;
    
    public int PinNumber() => -1;
}