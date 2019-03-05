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

namespace NmeaParser.Nmea.Gps.Garmin
{
    /// <summary>
    /// Altitude Information
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pgrmz")]
    [NmeaMessageType("PGRMZ")]
    public class Pgrmz : NmeaMessage
    {
        /// <summary>
        /// Altitude unit
        /// </summary>
        public enum AltitudeUnit
        {
            /// <summary>
            /// Unknown
            /// </summary>
            Unknown,
            /// <summary>
            /// Feet
            /// </summary>
            Feet
        }
        /// <summary>
        /// Position Fix Dimension
        /// </summary>
        public enum PositionFixType : int
        {
            /// <summary>
            /// Unknown
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// No fix
            /// </summary>
            NoFix = 1,
            /// <summary>
            /// 2D Fix
            /// </summary>
            Fix2D = 2,
            /// <summary>
            /// 3D Fix
            /// </summary>
            Fix3D = 3
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pgrmz"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Pgrmz(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 3)
                throw new ArgumentException("Invalid PGRMZ", "message");

            if (message[0].Length > 0)
                Altitude = double.Parse(message[0], CultureInfo.InvariantCulture);
            else
                Altitude = double.NaN;
            Unit = message[1] == "f" ? AltitudeUnit.Feet : AltitudeUnit.Unknown;
            int dim = -1;
            if (message[2].Length == 1 && int.TryParse(message[2], out dim))
            {
                if (dim >= (int)PositionFixType.NoFix && dim <= (int)PositionFixType.Fix3D)
                    FixType = (PositionFixType)dim;
            }
        }

        /// <summary>
        /// Current altitude
        /// </summary>
        public double Altitude { get; }

        /// <summary>
        /// Horizontal Error unit ('f' for Meters)
        /// </summary>
        public AltitudeUnit Unit { get; }

        /// <summary>
        /// Fix type
        /// </summary>
        public PositionFixType FixType { get; }
    }
}