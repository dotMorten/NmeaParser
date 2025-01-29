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
using System.Linq;

namespace NmeaParser.Messages
{
    /// <summary>
    ///     PTNL,GGK: Time, position, position type, DOP
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         1.: UTC time of position fix, in hhmmss.ss format. Hours must be two numbers, so may be padded. For example, 7 is shown as 07.
    ///         2.: UTC date of position fix, in mmddyy format. Day must be two numbers, so may be padded. For example, 8 is shown as 08.
    ///         3.: Latitude, in degrees and decimal minutes (dddmm.mmmmmmm)
    ///         4.: Direction of latitude: N: North S: South
    ///         5.: Longitude, in degrees and decimal minutes (dddmm.mmmmmmm). Should contain three digits of ddd.
    ///         6.: Direction of longitude: E: East W: West
    ///         7.: 	GPS Quality indicator:
    ///                 0: Fix not available or invalid
    ///                 1: Autonomous GPS fix
    ///                 2: RTK float solution
    ///                 3: RTK fix solution
    ///                 4: Differential, code phase only solution(DGPS)
    ///                 5: SBAS solution – WAAS/EGNOS/MSAS
    ///                 6: RTK float or RTK location 3D Network solution
    ///                 7: RTK fixed 3D Network solution
    ///                 8: RTK float or RTK location 2D in a Network solution
    ///                 9: RTK fixed 2D Network solution
    ///                 10: OmniSTAR HP / XP solution
    ///                 11: OmniSTAR VBS solution
    ///                 12: Location RTK solution
    ///                 13: Beacon DGPS
    ///                 14: CenterPoint RTX
    ///                 15: xFill
    ///         8.: Number of satellites in fix
    ///         9.: Dilution of Precision of fix (DOP)
    ///         10.: Ellipsoidal height of fix (antenna height above ellipsoid).
    ///         11.: M: ellipsoidal height is measured in meters
    ///     </para>
    ///     <para>
    ///         NOTE – The PTNL,GGK message is longer than the NMEA-0183 standard of 80 characters
    ///         NOTE – Even if a user-defined geoid model, or an inclined plane is loaded into the receiver, then the height output in the NMEA GGK string is always an ellipsoid height, for example, EHT24.123.
    ///     </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ggk")]
    [NmeaMessageType("GGK")]
    public class Ggk : NmeaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ggk"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Ggk(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 11)
                throw new ArgumentException("Invalid Ggk", "message");

            UtcTime = StringsToUtcDateTime(message[1], message[0]);
            Latitude = StringToLatitude(message[2], message[3]);
            Longitude = StringToLongitude(message[4], message[5]);
            if (!string.IsNullOrEmpty(message[6]))
                Quality = (Ggk.QualityIndicator)int.Parse(message[6], CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(message[7]))
                NumberOfSatellites = int.Parse(message[7], CultureInfo.InvariantCulture);
            DilutionOfPrecision = StringToDouble(message[8]);
            EllipsoidalHeightOfFix = StringToDouble(message[9]);
            EllipsoidalHeightIsMeasuredInMeters = message[10] == "M";
        }

        /// <summary>
        ///     UTC time of position fix, in DateTime format.
        ///     DateTime(fullYear, month, day, hours, minutes, seconds, milliseconds, DateTimeKind.Utc)
        /// </summary>
        public DateTime? UtcTime { get; }

        /// <summary>
        ///     Latitude, in degrees and decimal minutes (dddmm.mmmmmmm)
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        ///     Longitude, in degrees and decimal minutes (dddmm.mmmmmmm)
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        ///     GPS Quality indicator
        /// </summary>
        public Ggk.QualityIndicator Quality { get; }

        /// <summary>
        ///     Number of satellites in fix
        /// </summary>
        public int NumberOfSatellites { get; }

        /// <summary>
        ///     Dilution of Precision of fix (DOP)
        /// </summary>
        public double DilutionOfPrecision { get; }

        /// <summary>
        ///     Ellipsoidal height of fix (antenna height above ellipsoid). Must start with EHT.
        /// </summary>
        public double EllipsoidalHeightOfFix { get; }

        /// <summary>
        ///     M: ellipsoidal height is measured in meters
        /// </summary>
        public bool EllipsoidalHeightIsMeasuredInMeters { get; }

        /// <summary>
        ///     GPS Quality indicator
        /// </summary>
        public enum QualityIndicator : int
        {
            /// <summary>Fix not available or invalid</summary>
            Invalid = 0,
            /// <summary>Autonomous GPS fix</summary>
            GpsFix = 1,
            /// <summary>RTK float solution</summary>
            RtkFloat = 2,
            /// <summary>RTK fix solution</summary>
            RtkFix = 3,
            /// <summary>Differential, code phase only solution(DGPS)</summary>
            Ggps = 4,
            /// <summary>SBAS solution – WAAS/EGNOS/MSAS</summary>
            Sbas = 5,
            /// <summary>RTK float or RTK location 3D Network solution</summary>
            RtkFloatOrLocation3DNetworkSolution = 6,
            /// <summary>RTK fixed 3D Network solution</summary>
            RtkFixed3DNetworkSolution = 7,
            /// <summary>RTK float or RTK location 2D in a Network solution</summary>
            RtkFloatOrLocation2DNetworkSolution = 8,
            /// <summary>RTK fixed 2D Network solution</summary>
            RtkFixed2DNetworkSolution = 9,
            /// <summary>OmniSTAR HP / XP solution</summary>
            OmistarHpXp = 10,
            /// <summary>OmniSTAR VBS solution</summary>
            OmniStarVbs = 11,
            /// <summary>Location RTK solution</summary>
            LocationRtk = 12,
            /// <summary>Beacon DGPS</summary>
            BeaconDgps = 13,
            /// <summary>CenterPoint RTX</summary>
            CenterPointRtx = 14,
            /// <summary>xFill</summary>
            XFill = 15,
        }
    }
}