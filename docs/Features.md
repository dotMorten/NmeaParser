# Features

- Most common NMEA messages fully supported
  - GNSS: `BOD`, `GGA`, `GLL`, `GNS`, `GSA`, `GST`, `GSV`, `RMB`, `RMA`, `RMB`, `RMC`, `RTE`, `VTG`, `ZDA`
  - Garmin Proprietary: `PGRME`, `PGRMZ`
  - Trimble Laser Range Finder: `PTNLA`, `PTNLB`
  - TruePulse Laser Range Finder: `PLTIT`
- Automatic merging of multi-sentence messages for simplified usage.
- Extensible with custom NMEA messages. [See here](concepts/CustomMessages.md)
- Multiple input devices out of the box
  - System.IO.Stream (all platforms)
  - Emulation from NMEA log file (all platforms)
  - Serial Device: .NET Framework, .NET Core (Windows, Linux, Mac) and Windows Universal.
  - Bluetooth: Windows Universal and Android. .NET Core/.NET Framework is supported using the Bluetooth device via the SerialPortDevice.
- Devices support two-way communication, allowing you to enhance accuracy with [NTRIP](concepts/ntrip.md) or send other messages to your device.

