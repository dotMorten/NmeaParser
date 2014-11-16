Nmea Parser
=========

Library for handling data NMEA streams coming in from Bluetooth devices in Windows Store and Windows Phone, and Serial ports in Windows Desktop apps. Stream and File support is available for Xamarin Android and iOS devices. 

There's also generic support for NMEA files (for simulation/playback) and raw streams.

This library makes it easy to connect and listen for NMEA messages from various  Devices in Windows Store, Windows Phone and Windows Desktop apps.

Currently supported NMEA messages:
- Generic GPS NMEA (GPRMC, GPGGA, GPGLL, GPGSA, GPGSCV, GPRMB, GPRMC, GPBOD, GPRTE)
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


Usage
=====================

Please see the [WIKI](http://www.github.com/dotMorten/NmeaParser/wiki) how to use it on the various platforms

Screenshots
=====================
Screenshots from Desktop sample app:

![sampleapp2](https://cloud.githubusercontent.com/assets/1378165/5062460/10cc3064-6d77-11e4-8365-1e9c7c346afc.png)
![sampleapp3](https://cloud.githubusercontent.com/assets/1378165/5062461/123adfc2-6d77-11e4-8573-1fe95fa0325f.png)

Map view using the [ArcGIS Runtime](http://developer.arcgis.com/net) (see [separate branch](https://github.com/dotMorten/NmeaParser/tree/ArcGISLocationProvider)):
![sampleapp1](https://cloud.githubusercontent.com/assets/1378165/5062457/0e80e5f2-6d77-11e4-8013-75f94326ada7.jpg)

