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
    /// Fixes data for single or combined (GPS, GLONASS, possible future satellite systems, and systems combining these) satellite navigation systems
    /// </summary>
    /// <remarks>
    /// <para>This sentence provides fix data for GPS, GLONASS, BDS, QZSS, NavIC (IRNSS) and possible future satellite systems, and systems combining these.
    /// This sentence could be used with the talker identification of <see cref="Talker.GlobalPositioningSystem"/> for GPS, <see cref="Talker.GlonassReceiver"/> for GLONASS,
    /// <see cref="Talker.GalileoPositioningSystem"/> for Galileo, <see cref="Talker.BeiDouNavigationSatelliteSystem"/> for BDS, <see cref="Talker.QuasiZenithSatelliteSystem"/> for QZSS,
    /// <see cref="Talker.IndianRegionalNavigationSatelliteSystem"/> for NavIC (IRNSS), and <see cref="Talker.GlobalNavigationSatelliteSystem"/> for GNSS combined systems, as well as future identifiers.
    /// </para>
    /// <para>
    /// If a GNSS receiver is capable simultanously of producing a position using combined satellite systems, as well as a position using only one of the satellite systems, then separate GNS sentences
    /// with different <see cref="NmeaMessage.TalkerId"/> may be used to report the data calculated from the individual systems.
    /// </para>
    /// <para>
    /// If a GNSS receiver is set up to use more than one satellite system, but for some reason one or more of the systems are not available, then it may continue to report the positions
    /// using <c>GNGNS</c>, and use the <see cref="GpsModeIndicator"/> to show which satellit esystems are being used.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gns")]
    [NmeaMessageType("--GNS")]
    public class Gns : NmeaMessage, ITimestampedMessage
    {
        /*
         * Example of GNS messages:
         * $GNGNS,014035.00,4332.69262,S,17235.48549,E,RR,13,0.9,25.63,11.24,,*70   //GLONASS
         * $GPGNS,014035.00,,,,,,8,,,,1.0,23*76                                     //GPS
         * $GLGNS,014035.00,,,,,,5,,,,1.0,23*67                                     //GALILEO
         */

        /// <summary>
        /// GNS Mode Indicator
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// No fix. Satellite system not used in position fix, or fix not valid
            /// </summary>
            NoFix,
            /// <summary>
            /// Autonomous. Satellite system used in non-differential mode in position fix
            /// </summary>
            Autonomous,
            /// <summary>
            /// Differential (including all OmniSTAR services). Satellite system used in differential mode in position fix
            /// </summary>
            Differential,
            /// <summary>
            /// Precise. Satellite system used in precision mode. Precision mode is defined as no deliberate degradation (such as Selective Availability) and higher resolution code (P-code) is used to compute position fix.
            /// </summary>
            Precise,
            /// <summary>
            ///  Real Time Kinematic. Satellite system used in RTK mode with fixed integers
            /// </summary>
            RealTimeKinematic,
            /// <summary>
            /// Float RTK. Satellite system used in real time kinematic mode with floating integers
            /// </summary>
            FloatRtk,
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
            Simulator
        }

        /// <summary>
        /// Navigational status
        /// </summary>
        public enum NavigationalStatus
        {
            /// <summary>
            /// Navigational status not valid, equipment is not providing navigational status indication.
            /// </summary>
            NotValid = 0,
            /// <summary>
            /// Safe: When the estimated positioning accuracy (95% confidence) is within the selected accuracy level corresponding
            /// to the actual navigation mode, and integrity is available and within the requirements for the actual navigation mode,
            /// and a new valid position has been calculated within 1s for a conventional craft, and 0.5s for a high speed craft.
            /// </summary>
            Safe = 3,
            /// <summary>
            /// Caution: When integrity is not available
            /// </summary>
            Caution = 2,
            /// <summary>
            /// Unsafe When the estimated positioning accuracy (95% confidence) is less than the selected accuracy level corresponding
            /// to the actual navigation mode, and integrity is available and within the requirements for the actual navigation mode,
            /// and/or a new valid position has not been calculated within 1s for a conventional craft, and 0.5s for a high speed craft.
            /// </summary>
            Unsafe = 1
        }

        private static Mode ParseModeIndicator(char c)
        {
            switch (c)
            {
                case 'A': return Mode.Autonomous;
                case 'D': return Mode.Differential;
                case 'P': return Mode.Precise;
                case 'R': return Mode.RealTimeKinematic;
                case 'F': return Mode.FloatRtk;
                case 'E': return Mode.Estimated;
                case 'M': return Mode.Manual;
                case 'S': return Mode.Simulator;
                case 'N':
                default: return Mode.NoFix;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Gns"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Gns(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 12)
                throw new ArgumentException("Invalid GNS", "message");
            FixTime = StringToTimeSpan(message[0]);
            Latitude = NmeaMessage.StringToLatitude(message[1], message[2]);
            Longitude = NmeaMessage.StringToLongitude(message[3], message[4]);
            ModeIndicators = message[5].Select(t => ParseModeIndicator(t)).ToArray();
            NumberOfSatellites = int.Parse(message[6], CultureInfo.InvariantCulture);
            Hdop = NmeaMessage.StringToDouble(message[7]);
            OrhometricHeight = NmeaMessage.StringToDouble(message[8]);
            GeoidalSeparation = NmeaMessage.StringToDouble(message[9]);
            var timeInSeconds = StringToDouble(message[10]);
            if (!double.IsNaN(timeInSeconds))
                TimeSinceLastDgpsUpdate = TimeSpan.FromSeconds(timeInSeconds);
            else
                TimeSinceLastDgpsUpdate = null;
            if (message[11].Length > 0)
                DgpsStationId = message[11];

            if (message.Length > 12)
            {
                switch (message[12])
                {
                    case "S": Status = NavigationalStatus.Safe; break;
                    case "C": Status = NavigationalStatus.Caution; break;
                    case "U": Status = NavigationalStatus.Unsafe; break;
                    case "V":
                    default: Status = NavigationalStatus.NotValid; break;
                }
            }
        }

        /// <summary>
        /// Time of day fix was taken
        /// </summary>
        public TimeSpan FixTime { get; }
        
        /// <summary>
        /// Latitude
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// Longitude
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Mode indicator for GPS
        /// </summary>
        public Mode GpsModeIndicator => ModeIndicators.Length > 0 ? ModeIndicators[0] : Mode.NoFix;

        /// <summary>
        /// Mode indicator for GLONASS
        /// </summary>
        public Mode GlonassModeIndicator => ModeIndicators.Length > 1 ? ModeIndicators[1] : Mode.NoFix;

        /// <summary>
        /// Mode indicator for Galileo
        /// </summary>
        public Mode GalileoModeIndicator => ModeIndicators.Length > 2 ? ModeIndicators[2] : Mode.NoFix;

        /// <summary>
        /// Mode indicator for Beidou (BDS)
        /// </summary>
        public Mode BDSModeIndicator => ModeIndicators.Length > 3 ? ModeIndicators[3] : Mode.NoFix;

        /// <summary>
        /// Mode indicator for QZSS
        /// </summary>
        public Mode QZSSModeIndicator => ModeIndicators.Length > 4 ? ModeIndicators[4] : Mode.NoFix;

        /// <summary>
        /// Mode indicator for NavIC (IRNSS)
        /// </summary>
        public Mode NavICModeIndicator => ModeIndicators.Length > 5 ? ModeIndicators[5] : Mode.NoFix;

        /// <summary>
        /// Mode indicator for future constallations
        /// </summary>
        public Mode[] ModeIndicators { get; }

        /// <summary>
        /// Number of satellites (SVs) in use
        /// </summary>
        public int NumberOfSatellites { get; }

        /// <summary>
        /// Horizontal Dilution of Precision (HDOP), calculated using all the satellites (GPS, GLONASS, and any future satellites) used in computing the solution reported in each GNS sentence.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hdop")]
        public double Hdop { get; }

        /// <summary>
        /// Orthometric height in meters (MSL reference)
        /// </summary>
        public double OrhometricHeight { get; }

        /// <summary>
        /// Geoidal separation in meters - the difference between the earth ellipsoid surface and mean-sea-level (geoid) surface defined by the reference datum used in the position solution<br/>
        /// '-' = mean-sea-level surface below ellipsoid.
        /// </summary>
        public double GeoidalSeparation { get; }

        /// <summary>
        ///  Age of differential data - <see cref="TimeSpan.MaxValue"/> if talker ID is GN, additional GNS messages follow with GP and/or GL Age of differential data
        /// </summary>
        public TimeSpan? TimeSinceLastDgpsUpdate { get; }

        /// <summary>
        /// eference station ID1, range 0000-4095 - Null if talker ID is GN, additional GNS messages follow with GP and/or GL Reference station ID
        /// </summary>
        public string? DgpsStationId { get; }

        /// <summary>
        /// Navigational status
        /// </summary>
        public NavigationalStatus Status { get; }

        TimeSpan ITimestampedMessage.Timestamp => FixTime;
    }
}
