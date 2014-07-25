Nmea Parser
=========

Library for handling data NMEA streams coming in from Bluetooth devices in Windows Store and Windows Phone, and Serial ports in Windows Desktop apps. 

There's also generic support for NMEA files (for simulation/playback) and raw streams.

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

Usage
=====================

Please see the [WIKI](http://www.github.com/dotMorten/NmeaParser/wiki) how to use it on the various platforms
