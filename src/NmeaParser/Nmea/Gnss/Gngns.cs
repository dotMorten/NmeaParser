using System;
using System.Collections.Generic;
using System.Text;

namespace NmeaParser.Nmea.Gps
{
    /// <summary>
    /// Fixes data for single or combined (GPS, GLONASS, possible future satellite systems, and systems combining these) satellite navigation systems
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gngns")]
    [NmeaMessageType("GNGNS")]
    public class Gngns : Gns
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gngns"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Gngns(string type, string[] message) : base(type, message) { }
    }
}