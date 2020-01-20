# Creating a Serial Port device in a .NET Framework

```csharp
var port = new System.IO.Ports.SerialPort("COM3", 9600); //change name and baud rage to match your serial port
var device = new NmeaParser.SerialPortDevice(port);
device.MessageReceived += device_NmeaMessageReceived;
device.OpenAsync();
...
private void device_NmeaMessageReceived(NmeaParser.NmeaDevice sender, NmeaParser.NmeaMessageReceivedEventArgs args)
{
   // called when a message is received
}								
```

