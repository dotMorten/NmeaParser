using System;

namespace NmeaParser.Nmea.Gnss
{
    /// <summary>
    /// Position error statistics 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gngst")]
    [NmeaMessageType("GNGST")]
    public class Gngst : Gst
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gngst"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Gngst(string type, string[] message) : base(type, message) { }
    }
}