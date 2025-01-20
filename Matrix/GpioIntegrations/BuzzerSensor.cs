using System.Device.Gpio;

namespace Matrix.GpioIntegrations;

public class BuzzerSensor : IBuzzerSensor
{
    private readonly GpioController _controller;

    private int _buzzerPin;
    
    public BuzzerSensor(GpioController controller, int pin)
    {
        _controller = controller;
        _buzzerPin = pin;
        
        _controller.OpenPin(_buzzerPin, PinMode.Output);
    }
    
    public int PinNumber() => _buzzerPin;

    private void ToggleBuzz(PinValue value) => _controller.Write(_buzzerPin, value);

    public void Buzz(bool onOff)
    {
        ToggleBuzz(onOff ? PinValue.High : PinValue.Low);
    }

    public bool Status() => _controller.Read(_buzzerPin) == PinValue.High;
    
    public void EnsureOff()
    {
        if (_controller.Read(_buzzerPin) == PinValue.High)
        {
            Buzz(false);
        }
    }
}