using System;
using System.Collections.Generic;
using System.Text;

namespace NmeaParser.Nmea.Glonass
{
    /// <summary>
    /// Fix data for GLONASS satellite navigation systems
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Glgns")]
    [NmeaMessageType("GLGNS")]
    public class Glgns : Gns
    {
    }
}
