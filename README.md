Nmea Parser
=========

_NOTE: The documentation below matches the v1.x releases. I'm currently working on a big revamp of the API to make it simpler and cover more scenarios learned over time, which will include some breaking changes. So the doc below does not currently match what you see in the master-branch_

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

Example:
```csharp
[NmeaMessageType("GPRMC")]
public class Gprmc : NmeaMessage
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

