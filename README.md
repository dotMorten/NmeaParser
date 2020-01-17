Nmea Parser
=========

Library for reading and parsing NMEA data message streams.
It makes it easy to connect and listen for NMEA messages coming from various devices in Windows Universal, Windows Desktop/.NET and Windows Universal apps as well as Xamarin for iOS and Android.

## Sponsoring

If you like this library and use it a lot, consider sponsoring me. Anything helps and encourages me to keep going.

See here for details: https://github.com/sponsors/dotMorten

### Features
The following inputs are supported:
- System.IO.Stream (all platforms)
- Emulation from NMEA log file (all platforms)
- Bluetooth: Windows Universal and Android. Desktop is supported using the bluetooth device via the SerialPortDevice.
- Serial Device: Windows Desktop and Windows Universal.


Currently supported NMEA messages:
- GNSS: BOD, GGA, GLL, GNS, GSA, GST, GSV, RMB, RMA, RMB, RMC, RTE, VTG, ZDA
- Garmin Proprietary: PGRME, PGRMZ
- Trimble Laser Range Finder: PTNLA, PTNLB
- TruePulse Laser Range Finder: PLTIT

The API is easily extensible with more NMEA messages. Simply create a new class inheriting from "NmeaMessage" and use the NmeaMessageType Attribute to tag it with the NMEA Message Token it supports.

Example:
```csharp
[NmeaMessageType("--RMC")]
public class Rmc : NmeaMessage
{
	protected override void LoadMessage(string[] message)
	{
		//TODO: Process message parts
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

