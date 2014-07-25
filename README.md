Nmea Parser
=========

Library for handling data NMEA streams coming in from Bluetooth devices in Windows Store and Windows Phone.

This library makes it easy to connect and listen for NMEA messages from Bluetooth Devices in Windows Store and Windows Phone apps.

Currently supported devices:
- Generic GPS NMEA
- Trimble Laser Range Finder
- TruePulse Laser Range Finder

The API is easily extensible with more NMEA message. Simply create a new class inheriting from "NmeaMessage" and use the NmeaMessageType Attribute to tag it with the NMEA Message Token it supports.

Example:
```
[NmeaMessageType(Type = "GPRMC")]
public class Gprmc : NmeaMessage
{
	protected override void LoadMessage(string[] message)
	{
		//Process message parts
	}
}
```

If you add new messages, please fork, provide a simple unit test for the message and submit a pull request.

Usage - Windows Store
=====================
Ensure the bluetooth capability is enabled by opening package.appxmanifest in a text editor, and add the following to the <Capabilities> section:
```
    <m2:DeviceCapability Name="bluetooth.rfcomm">
      <m2:Device Id="any">
        <m2:Function Type="name:serialPort" />
      </m2:Device>
    </m2:DeviceCapability>
```
See more here on bluetooth device capabilities in WinStore: http://msdn.microsoft.com/en-us/library/windows/apps/dn263090.aspx

```
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
	await rangeFinder.StartAsync();				
}
...
private void device_NmeaMessageReceived(NmeaParser.NmeaDevice sender, NmeaParser.Nmea.NmeaMessage args)
{
   // called when a message is received
}

```


Usage - Windows Phone
======================
Ensure the "ID_CAP_PROXIMITY" capability is enabled in the WMAppManifest.xml file.
```

// Search for all paired devices
PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
//Get all devices
var devices = await PeerFinder.FindAllPeersAsync();
//Select device by name (in this case TruePulse 360B Laser Range Finder)
var TruePulse360B = devices.Where(t => t.DisplayName.StartsWith("TP360B-")).FirstOrDefault();
if (TruePulse360B != null)
{
	var device = new NmeaParser.BluetoothDevice((PeerInformation)TruePulse360B);
	device.MessageReceived += device_NmeaMessageReceived;
}
...
private void device_NmeaMessageReceived(NmeaParser.NmeaDevice sender, NmeaParser.Nmea.NmeaMessage args)
{
   // called when a message is received
}								
```

Usage - Windows Desktop
======================
```
	var port = new System.IO.Ports.SerialPort("COM3", 9600);
	var device = new NmeaParser.SerialPortDevice(port);
	device.MessageReceived += device_NmeaMessageReceived;
...
private void device_NmeaMessageReceived(NmeaParser.NmeaDevice sender, NmeaParser.Nmea.NmeaMessage args)
{
   // called when a message is received
}								
```


