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
        /// <summary>
        /// Initializes a new instance of the <see cref="Glgns"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Glgns(string type, string[] message) : base(type, message) { }
    }
}
