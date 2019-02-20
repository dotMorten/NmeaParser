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
        /// <summary>
        /// Initializes a new instance of the <see cref="Gpgns"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Gpgns(string type, string[] message) : base(type, message) { }
    }
}
