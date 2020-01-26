# Getting started:

### 1. Install from NuGET:

You can get the library via [NuGet](http://www.nuget.org) if you have the extension installed for Visual Studio or via the PowerShell package manager.  This control is published via NuGet at [SharpGIS.NmeaParser](https://nuget.org/packages/SharpGIS.NmeaParser).

<table border="4px">
<tr><td>
            <code>PM&gt; Install-Package SharpGIS.NmeaParser</code>
</td></tr></table>

### 2. Create a new device:

```cs
 // Create one of the NMEA devices
 var device = new NmeaFileDevice("PathToNmeaLogFile.txt", 1000);
 // Listen to messages from the device: 
 device.MessageReceived += device_NmeaMessageReceived;
 // Open the device and start receiving:
 device.OpenAsync();

 // Create event handler for receiving messages:
 private void device_NmeaMessageReceived(NmeaDevice sender, NmeaMessageReceivedEventArgs args)
 {
    // called when a message is received
    if(args.Message is NmeaParser.Messages.Rmc rmc)
    {
        Console.WriteLine($"Your current location is: {rmc.Latitude} , {rmc.Longitude}");
    }
 }
```
See the Platform specific device creation section in the menu for more specifics on device creation.

### 3. [Browse the API Reference](../api/index.html)

### 4. Explore the Object Model Diagrams
[.NET Standard OMD](../api/omd.html)

### 5. Explore the samples on GitHub:

 - [Desktop app](https://github.com/dotMorten/NmeaParser/tree/master/src/SampleApp.WinDesktop) (.NET Framework / .NET Core)
 - [Windows Universal app](https://github.com/dotMorten/NmeaParser/tree/master/src/SampleApp.UWP)
 - [Android app](https://github.com/dotMorten/NmeaParser/tree/master/src/SampleApp.Droid)
