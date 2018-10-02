using System;
using System.Globalization;

namespace NmeaParser.Nmea
{
    /// <summary>
    /// Date and time of fix
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zda")]
    public abstract class Zda : NmeaMessage
    {
        /// <summary>
        /// Called when the message is being loaded.
        /// </summary>
        /// <param name="message">The NMEA message values.</param>
        protected override void OnLoadMessage(string[] message)
        {
            if (message?.Length != 6)
            {
                throw new ArgumentException("Invalid GNGST", nameof(message));
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

        public DateTime FixDateTime { get; private set; }
    }
}
