# Creating a Serial Port device in a Windows Universal app

To use the NMEA Parser against a serial device in a Windows 10 Universal app, ensure the serial device capability is enabled by opening package.appxmanifest in a text editor, and add the following to the `<Capabilities>` section:
```xml
    <DeviceCapability Name="serialcommunication" > 
      <Device Id="any"> 
        <Function Type="name:serialPort"/> 
      </Device> 
    </DeviceCapability> 
```

```csharp
var selector = SerialDevice.GetDeviceSelector("COM3"); //Get the serial port on port '3'
var devices = await DeviceInformation.FindAllAsync(selector);
if(devices.Any()) //if the device is found
{
	var deviceInfo = devices.First();
	var serialDevice = await SerialDevice.FromIdAsync(deviceInfo.Id);
	//Set up serial device according to device specifications:
	//This might differ from device to device
	serialDevice.BaudRate = 4800;
	serialDevice.DataBits = 8;
	serialDevice.Parity = SerialParity.None;
	var device = new NmeaParser.SerialPortDevice(serialDevice);
	device.MessageReceived += device_NmeaMessageReceived;
}
...
private void device_NmeaMessageReceived(NmeaParser.NmeaDevice sender, NmeaMessageReceivedEventArgs args)
{
   // called when a message is received
}
```
