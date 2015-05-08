using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NmeaParser.Nmea.Gps
{
    /// <summary>
    ///  Global Positioning System Fix Data -> GPS & Glonas. Structure is the same as GPGGA
    /// </summary>
    [NmeaMessageType(Type = "GNGGA")]
    public class Gngga : Gpgga
    {
    }
}
