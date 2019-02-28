Nmea Parser
=========

Library for reading and parsing NMEA data message streams.
It makes it easy to connect and listen for NMEA messages coming from various devices in Windows Store, Windows Phone, Windows Desktop/.NET and Windows Universal apps as well as Xamarin for iOS and Android.

The following inputs are supported:
- System.IO.Stream (all platforms)
- Emulation from NMEA log file (all platforms)
- Bluetooth: Windows Universal. Desktop is supported using the bluetooth device via the SerialPortDevice.
- Serial Device: Windows Desktop and Windows Universal.


Currently supported NMEA messages:
- GPS: GPBOD, GPGGA, GPGLL, GPGNS, GPGSA, GPGST, GPGSV, GPRMB, GPRMC, GPRTE
- GLONASS: GLGNS, GLGSV
- GALILEO: GAGSV
- Generic GNSS: GNGGA, GNGLL, GNGNS, GNGSA, GNGST, GNRMC
- Garmin GPS NMEA (PGRME, PGRMZ)
- Trimble Laser Range Finder (PTNLA, PTNLB)
- TruePulse Laser Range Finder (PLTIT)

The API is easily extensible with more NMEA messages. Simply create a new class inheriting from "NmeaMessage" and use the NmeaMessageType Attribute to tag it with the NMEA Message Token it supports.

### Examples:

**Add a new data type**
```csharp
[NmeaMessageType("GPRMC")]
public class Gprmc : NmeaMessage
{
    public Gprmc(string type, string[] message) : base(type, message)
    {
        //TODO: Process message parts
    }
}
```

**Connect to a serial gps gevice on COM1**
```csharp
static void Main(string[] args)
{
    using (var device = new NmeaParser.SerialPortDevice(new SerialPort("COM1")))
    {
        device.MessageReceived += Device_MessageReceived;
        device.OpenAsync().GetAwaiter().GetResult();
        Console.ReadLine();
        device.MessageReceived -= Device_MessageReceived;
    }
}

static void ProcessMessage(NmeaMessage message)
{
    var messageType = message.GetType();
    if (messageType == typeof(NmeaParser.Nmea.Gnss.Gngll))
    {
        var gngll = message as NmeaParser.Nmea.Gnss.Gngll;
        Console.WriteLine($"Gngll - {gngll.Latitude} {gngll.Longitude}");
    }
    else if (messageType == typeof(NmeaParser.Nmea.Gnss.Gngga))
    {
        var gngga = message as NmeaParser.Nmea.Gnss.Gngga;
        Console.WriteLine($"Gngga - {gngga.Latitude} {gngga.Longitude}");
    }
}
```

If you add new messages, please fork, provide a simple unit test for the message and submit a pull request.


### NuGet
You can get the library via [NuGet](http://www.nuget.org) if you have the extension installed for Visual Studio or via the PowerShell package manager.  This control is published via NuGet at [SharpGIS.NmeaParser](https://nuget.org/packages/SharpGIS.NmeaParser).

<table border="4px">
<tr><td>
            <code>PM&gt; Install-Package SharpGIS.NmeaParser</code>
</td></tr></table>

Usage
=====================

Please see the [WIKI](http://www.github.com/dotMorten/NmeaParser/wiki) how to use it on the various platforms

Screenshots
=====================
Screenshots from Desktop sample app:

![sampleapp2](https://cloud.githubusercontent.com/assets/1378165/5062460/10cc3064-6d77-11e4-8365-1e9c7c346afc.png)
![sampleapp3](https://cloud.githubusercontent.com/assets/1378165/5062461/123adfc2-6d77-11e4-8573-1fe95fa0325f.png)

Map view using the [ArcGIS Runtime](http://developer.arcgis.com/net) (see [separate branch](https://github.com/dotMorten/NmeaParser/tree/ArcGISLocationProvider)):
![sampleapp1](https://cloud.githubusercontent.com/assets/1378165/5062617/3419eef4-6d7b-11e4-8c8b-a6c4eaa212f0.jpg)

