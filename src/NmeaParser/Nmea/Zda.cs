//  *******************************************************************************
//  *  Licensed under the Apache License, Version 2.0 (the "License");
//  *  you may not use this file except in compliance with the License.
//  *  You may obtain a copy of the License at
//  *
//  *  http://www.apache.org/licenses/LICENSE-2.0
//  *
//  *   Unless required by applicable law or agreed to in writing, software
//  *   distributed under the License is distributed on an "AS IS" BASIS,
//  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *   See the License for the specific language governing permissions and
//  *   limitations under the License.
//  ******************************************************************************

using System;
using System.Globalization;

namespace NmeaParser.Messages
{
    /// <summary>
    /// Date and time of fix
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zda")]
    [NmeaMessageType("--ZDA")]
    public class Zda : NmeaMessage, ITimestampedMessage
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
            if (int.TryParse(message[1], NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out int day) &&
                int.TryParse(message[2], NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out int month) &&
                int.TryParse(message[3], NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out int year))
            {
                FixDateTime = new DateTimeOffset(year, month, day, time.Hours, time.Minutes,
                    time.Seconds, TimeSpan.Zero);
            }
            // Index 4 and 5 is used to specify a local time zone.
            // However I haven't come across any receiver that actually
            // specify this, so we're just ignoring it.
        }

        /// <summary>
        /// Gets the time of fix
        /// </summary>
        public DateTimeOffset FixDateTime { get; }

        TimeSpan ITimestampedMessage.Timestamp => FixDateTime.TimeOfDay;
    }
}
