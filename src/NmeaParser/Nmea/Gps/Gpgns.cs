using System;
using System.Collections.Generic;
using System.Text;

namespace NmeaParser.Nmea.Gps
{
    /// <summary>
    /// Fixes data for GPS satellite navigation systems
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpgns")]
    [NmeaMessageType("GPGNS")]
    public class Gpgns : Gns
    {
    }
}
