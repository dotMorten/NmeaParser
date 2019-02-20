namespace NmeaParser.Nmea.Gnss
{
    /// <summary>
    /// Date and time of fix
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gnzda")]
    [NmeaMessageType("GNZDA")]
    public class Gnzda : Zda
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gnzda"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Gnzda(string type, string[] message) : base(type, message) { }
    }
}
