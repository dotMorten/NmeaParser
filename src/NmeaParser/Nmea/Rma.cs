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
    /// Recommended minimum specific Loran-C Data
    /// </summary>
    /// <remarks>
    /// <para>Position, course and speed data provided by a Loran-C receiver. Time differences A and B are those used in computing latitude/longitude.
    /// This sentence is transmitted at intervals not exceeding 2-seconds and is always accompanied by <see cref="Rmb"/> when a destination waypoint is active.</para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gprmb")]
    [NmeaMessageType("--RMA")]
    public class Rma : NmeaMessage, IGeographicLocation
    {
        /// <summary>
        /// Positioning system status field
        /// </summary>
        public enum PositioningStatus
        {
            /// <summary>
            /// Data not valid
            /// </summary>
            Invalid = 0,
            /// <summary>
            /// Autonomous
            /// </summary>
            Autonomous,
            /// <summary>
            /// Differential
            /// </summary>
            Differential
        }

        /// <summary>
        /// Positioning system mode indicator
        /// </summary>
        public enum PositioningMode
        {
            /// <summary>
            /// Data not valid
            /// </summary>
            NotValid = 0,
            /// <summary>
            /// Autonomous mode
            /// </summary>
            Autonomous,
            /// <summary>
            /// Differential mode
            /// </summary>
            Differential,
            /// <summary>
            /// Estimated (dead reckoning) mode
            /// </summary>
            Estimated,
            /// <summary>
            /// Manual input mode
            /// </summary>
            Manual,
            /// <summary>
            /// Simulator mode
            /// </summary>
            Simulator,
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Rma"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Rma(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 12)
                throw new ArgumentException("Invalid RMA", "message");

            Status = message[0] == "A" ? PositioningStatus.Autonomous : (message[0] == "D" ? PositioningStatus.Differential : PositioningStatus.Invalid);
            Latitude = NmeaMessage.StringToLatitude(message[1], message[2]);
            Longitude = NmeaMessage.StringToLongitude(message[3], message[4]);
            if (double.TryParse(message[5], NumberStyles.Float, CultureInfo.InvariantCulture, out double tmp))
                TimeDifferenceA = TimeSpan.FromMilliseconds(tmp / 1000);
            if (double.TryParse(message[6], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
                TimeDifferenceB = TimeSpan.FromMilliseconds(tmp / 1000);
            if (double.TryParse(message[7], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
                Speed = tmp;
            else
                Speed = double.NaN;
            if (double.TryParse(message[8], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
                Course = tmp;
            else
                Course = double.NaN;
            if (double.TryParse(message[9], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
                MagneticVariation = tmp * (message[10] == "E" ? -1 : 1);
            else
                MagneticVariation = double.NaN;

            switch (message[11])
            {
                case "A": Mode = PositioningMode.Autonomous; break;
                case "D": Mode = PositioningMode.Autonomous; break;
                case "E": Mode = PositioningMode.Estimated; break;
                case "M": Mode = PositioningMode.Manual; break;
                case "S": Mode = PositioningMode.Simulator; break;
                case "N":
                default:
                    Mode = PositioningMode.Autonomous; break;
            }
        }

        /// <summary>
        /// Positioning system status
        /// </summary>
        public PositioningStatus Status { get; }

        /// <summary>
        /// Latitude
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// Longitude
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Time difference A
        /// </summary>
        public TimeSpan TimeDifferenceA { get; }

        /// <summary>
        /// Time difference B
        /// </summary>
        public TimeSpan TimeDifferenceB { get; }

        /// <summary>
        /// Speed over ground in knots.
        /// </summary>
        public double Speed { get; }

        /// <summary>
        /// Course over ground in degrees from true north
        /// </summary>
        public double Course { get; }

        /// <summary>
        /// Magnetic variation in degrees.
        /// </summary>
        public double MagneticVariation { get; }

        /// <summary>
        /// Positioning system mode indicator
        /// </summary>
        public PositioningMode Mode { get; }
    }
}
