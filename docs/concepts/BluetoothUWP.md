# Creating a Bluetooth device in a Windows Universal App

To use the NMEA Parser against a bluetooth device in a Windows Store or Windows Phone WinRT/Universal App, ensure the bluetooth capability is enabled by opening package.appxmanifest in a text editor, and add the following to the `<Capabilities>` section:
```xml
    <DeviceCapability Name="bluetooth.rfcomm">
      <Device Id="any">
        <Function Type="name:serialPort" />
      </Device>
    </DeviceCapability>
```
See more here on bluetooth device capabilities in WinStore: http://msdn.microsoft.com/en-us/library/windows/apps/dn263090.aspx

Make sure your Bluetooth device is paired with your Windows Device.

```csharp
//Get list of devices
string serialDeviceType = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
var devices = await DeviceInformation.FindAllAsync(serialDeviceType);
//Select device by name (in this case TruePulse 360B Laser Range Finder)
var TruePulse360B = devices.Where(t => t.Name.StartsWith("TP360B-")).FirstOrDefault();
//Get service
RfcommDeviceService rfcommService = await RfcommDeviceService.FromIdAsync(TruePulse360B.Id);
if (rfcommService != null)
{
	var rangeFinder = new NmeaParser.BluetoothDevice(rfcommService);
	rangeFinder.MessageReceived += device_NmeaMessageReceived;
	await rangeFinder.OpenAsync();				
}
...
private void device_NmeaMessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs args)
{
   // called when a message is received
}
```
