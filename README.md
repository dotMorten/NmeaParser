####Note!

This fork is downgraded from .NET Framework 4.5 to target 4.0.

###Nmea Parser

Library for handling data NMEA streams coming in from Bluetooth devices in Windows Store and Windows Phone, and Serial ports in Windows Desktop apps. 

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
[NmeaMessageType(Type = "GPRMC")]
public class Gprmc : NmeaMessage
{
	protected override void LoadMessage(string[] message)
	{
		//TODO: Process message parts
	}
}
```

###Usage

Please see the [WIKI](http://www.github.com/dotMorten/NmeaParser/wiki) how to use it on the various platforms
