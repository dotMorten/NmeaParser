namespace NmeaParser.Nmea.Glonass
{
    /// <summary>
    /// Date and time of fix
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Glzda")]
    [NmeaMessageType("GLZDA")]
    public class Glzda : Zda
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Glzda"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Glzda(string type, string[] message) : base(type, message) { }
    }
}