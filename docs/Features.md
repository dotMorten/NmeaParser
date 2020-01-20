# Features

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

