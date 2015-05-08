using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NmeaParser.Nmea.Gps
{
    [NmeaMessageType(Type = "GNRMC")]
    public class Gnrmc : Gprmc
    {
    }
}
