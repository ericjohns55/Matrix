using System.Device.Gpio;

namespace Matrix.GpioIntegrations;

public class BuzzerSensor
{
    private readonly GpioController _controller;
    private int _buzzerPin;
    private bool _isOn;
    
    public BuzzerSensor(GpioController controller, int pin)
    {
        _controller = controller;
        _buzzerPin = pin;
        
        _controller.OpenPin(_buzzerPin, PinMode.Output);
    }

    private void ToggleBuzz(PinValue value) => _controller.Write(_buzzerPin, value);

    public void BuzzOn()
    {
        ToggleBuzz(PinValue.High);
        _isOn = true;
    }

    public void BuzzOff()
    {
        ToggleBuzz(PinValue.Low);
        _isOn = false;
    }

    public void EnsureOff()
    {
        if (_isOn)
        {
            BuzzOff();
        }
    }
}