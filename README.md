Nmea Parser
=========

Library for reading and parsing NMEA data message streams.
It makes it easy to connect and listen for NMEA messages coming from various devices in Windows Universal, Windows Desktop/.NET and Windows Universal apps as well as Xamarin/.NET for iOS and Android.


## Sponsoring

If you like this library and use it a lot, consider sponsoring me. Anything helps and encourages me to keep going.

See here for details: https://github.com/sponsors/dotMorten


### Documentation

Read the full documentation here: https://dotmorten.github.io/NmeaParser/


### Features

- Most common NMEA messages fully supported
  - GNSS: BOD, GGA, GLL, GNS, GSA, GST, GSV, RMB, RMA, RMB, RMC, RTE, VTG, ZDA
  - Garmin Proprietary: PGRME, PGRMZ
  - Trimble Laser Range Finder: PTNLA, PTNLB
  - TruePulse Laser Range Finder: PLTIT
- Automatic merging of multi-sentence messages for simplified usage.
- Extensible with custom NMEA messages [see here](concepts/CustomMessages.html)
- Multiple input devices out of the box
  - System.IO.Stream (all platforms)
  - Emulation from NMEA log file (all platforms)
  - Serial Device: .NET Framework, .NET Core (Windows, Linux, Mac) and Windows Universal.
  - Bluetooth: Windows Universal and Android. .NET Core/.NET Framework is supported using the bluetooth device via the SerialPortDevice.


### NuGet
You can get the library via [NuGet](http://www.nuget.org) if you have the extension installed for Visual Studio or via the PowerShell package manager.  This control is published via NuGet at [SharpGIS.NmeaParser](https://nuget.org/packages/SharpGIS.NmeaParser).

<table border="4px">
<tr><td>
            <code>PM&gt; Install-Package SharpGIS.NmeaParser</code>
</td></tr></table>

Usage
=====================

Please see the [Documentation](https://dotmorten.github.io/NmeaParser/concepts/index.html) on how to use it on the various platforms.

Screenshots
=====================
Screenshots from Desktop sample app:

![sampleapp2](https://cloud.githubusercontent.com/assets/1378165/5062460/10cc3064-6d77-11e4-8365-1e9c7c346afc.png)
![sampleapp3](https://cloud.githubusercontent.com/assets/1378165/5062461/123adfc2-6d77-11e4-8573-1fe95fa0325f.png)


