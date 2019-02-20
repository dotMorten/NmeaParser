namespace NmeaParser.Nmea.Gps
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpzda")]
    [NmeaMessageType("GPZDA")]
    public class Gpzda : Zda
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gpzda"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Gpzda(string type, string[] message) : base(type, message) { }
    }
}
