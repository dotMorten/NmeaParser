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
    /// Recommended minimum navigation information
    /// </summary>
    /// <remarks>
    /// <para>Navigation data from present position to a destination waypoint provided by a Loran-C, GNSS, DECCA, navigatin computer
    /// or other integrated navigation system.</para>
    /// <para>
    /// This sentence always accompanies <see cref="Rma"/> and <see cref="Rmc"/> sentences when a destination is active when provided by a Loran-C or GNSS receiver,
    /// other systems may transmit <see cref="Rmb"/> without <see cref="Rma"/> or <see cref="Rmc"/>.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gprmb")]
    [NmeaMessageType("--RMB")]
    public class Rmb : NmeaMessage
    {
        /// <summary>
        /// Data status
        /// </summary>
        public enum DataStatus
        {
            /// <summary>
            /// Ok
            /// </summary>
            Ok,
            /// <summary>
            /// Warning
            /// </summary>
            Warning
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rmb"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Rmb(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 13)
                throw new ArgumentException("Invalid GPRMB", "message");

            Status = message[0] == "A" ? DataStatus.Ok : Rmb.DataStatus.Warning;
            double tmp;
            if (double.TryParse(message[1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
            {
                CrossTrackError = tmp;

                if (message[2] == "L") //Steer left
                    CrossTrackError *= -1;
            }
            else
                CrossTrackError = double.NaN;

            if (message[3].Length > 0)
                OriginWaypointId = int.Parse(message[3], CultureInfo.InvariantCulture);
            if (message[3].Length > 0)
                DestinationWaypointId = int.Parse(message[4], CultureInfo.InvariantCulture);
            DestinationLatitude = NmeaMessage.StringToLatitude(message[5], message[6]);
            DestinationLongitude = NmeaMessage.StringToLongitude(message[7], message[8]);
            if (double.TryParse(message[9], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
                RangeToDestination = tmp;
            else
                RangeToDestination = double.NaN;
            if (double.TryParse(message[10], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
                TrueBearing = tmp;
            else
                TrueBearing = double.NaN;
            if (double.TryParse(message[11], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
                Velocity = tmp;
            else
                Velocity = double.NaN;
            Arrived = message[12] == "A";
        }

        /// <summary>
        /// Data Status
        /// </summary>
        public DataStatus Status { get; }

        /// <summary>
        /// Cross-track error (steer left when negative, right when positive)
        /// </summary>
        public double CrossTrackError { get; }

        /// <summary>
        /// Origin waypoint ID
        /// </summary>
        public double OriginWaypointId { get; }

        /// <summary>
        /// Destination waypoint ID
        /// </summary>
        public double DestinationWaypointId { get; }

        /// <summary>
        /// Destination Latitude
        /// </summary>
        public double DestinationLatitude { get; }

        /// <summary>
        /// Destination Longitude
        /// </summary>
        public double DestinationLongitude { get; }

        /// <summary>
        /// Range to destination in nautical miles
        /// </summary>
        public double RangeToDestination { get; }

        /// <summary>
        /// True bearing to destination
        /// </summary>
        public double TrueBearing { get; }

        /// <summary>
        /// Velocity towards destination in knots
        /// </summary>
        public double Velocity { get; }

        /// <summary>
        /// Arrived (<c>true</c> if arrived)
        /// </summary>
        public bool Arrived { get; }
    }
}
