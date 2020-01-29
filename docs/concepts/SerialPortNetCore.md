# Creating a Serial Port device in a .NET Core app

```csharp
string portname = "COM3"; // Change to match the name of the port your device is connected to
int baudrate = 9600; // Change to the baud rate your device communicates at (usually specified in the manual)
var port = new System.IO.Ports.SerialPort(portname, baudrate);
var device = new NmeaParser.SerialPortDevice(port);
device.MessageReceived += OnNmeaMessageReceived;
device.OpenAsync();
...
private void OnNmeaMessageReceived(NmeaParser.NmeaDevice sender, NmeaParser.NmeaMessageReceivedEventArgs args)
{
   // called when a message is received
}								
```

