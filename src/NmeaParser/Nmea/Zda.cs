using System;
using System.Globalization;

namespace NmeaParser.Nmea
{
    /// <summary>
    /// Date and time of fix
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zda")]
    [NmeaMessageType("--ZDA")]
    public class Zda : NmeaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Zda"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Zda(string type, string[] message) : base(type, message)
        {
            if (message?.Length != 6)
            {
                throw new ArgumentException("Invalid ZDA", nameof(message));
            }

            var time = StringToTimeSpan(message[0]);
            var day = int.Parse(message[1], CultureInfo.InvariantCulture);
            var month = int.Parse(message[2], CultureInfo.InvariantCulture);
            var year = int.Parse(message[3], CultureInfo.InvariantCulture);

            FixDateTime = new DateTime(year, month, day, time.Hours, time.Minutes,
                time.Seconds, DateTimeKind.Utc);

            // Index 4 and 5 is used to specify a local time zone.
            // However I haven't come across any receiver that actually
            // specify this, so we're just ignoring it.
        }

        /// <summary>
        /// Gets the time of fix
        /// </summary>
        public DateTime FixDateTime { get; }
    }
}
