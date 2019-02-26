﻿//
// Copyright (c) 2014 Morten Nielsen
//
// Licensed under the Microsoft Public License (Ms-PL) (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://opensource.org/licenses/Ms-PL.html
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser.Nmea
{
    /// <summary>
    /// Fixes data for single or combined (GPS, GLONASS, possible future satellite systems, and systems combining these) satellite navigation systems
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gns")]
    [NmeaMessageType("--GNS")]
    public class Gns : NmeaMessage
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
            /// Not valid for navigation
            /// </summary>
            NotValid,
            /// <summary>
            /// Safe
            /// </summary>
            Safe,
            /// <summary>
            /// Caution
            /// </summary>
            Caution,
            /// <summary>
            /// Unsafe
            /// </summary>
            Unsafe
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
            if (message[5].Length > 0)
                GpsModeIndicator = ParseModeIndicator(message[5][0]);
            if (message[5].Length > 1)
                GlonassModeIndicator = ParseModeIndicator(message[5][1]);
            if (message[5].Length > 2)
            {
                FutureModeIndicator = message[5].Skip(2).Select(t => ParseModeIndicator(t)).ToArray();
            }
            else
                FutureModeIndicator = new Mode[] { };
            NumberOfSatellites = int.Parse(message[6], CultureInfo.InvariantCulture);
            Hdop = NmeaMessage.StringToDouble(message[7]);
            OrhometricHeight = NmeaMessage.StringToDouble(message[8]);
            GeoidalSeparation = NmeaMessage.StringToDouble(message[9]);
            var timeInSeconds = StringToDouble(message[10]);
            if (!double.IsNaN(timeInSeconds))
                TimeSinceLastDgpsUpdate = TimeSpan.FromSeconds(timeInSeconds);
            else
                TimeSinceLastDgpsUpdate = TimeSpan.MaxValue;
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
        /// <seealso cref="GlonassModeIndicator"/>
        /// <see cref="FutureModeIndicator"/>
        public Mode GpsModeIndicator { get; }

        /// <summary>
        /// Mode indicator for GLONASS
        /// </summary>
        /// <seealso cref="GpsModeIndicator"/>
        /// <see cref="FutureModeIndicator"/>
        public Mode GlonassModeIndicator { get; }

        /// <summary>
        /// Mode indicator for future constallations
        /// </summary>
        /// <seealso cref="GlonassModeIndicator"/>
        /// <seealso cref="GpsModeIndicator"/>
        public Mode[] FutureModeIndicator { get; }

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
		public TimeSpan TimeSinceLastDgpsUpdate { get; }

		/// <summary>
		/// eference station ID1, range 0000-4095 - Null if talker ID is GN, additional GNS messages follow with GP and/or GL Reference station ID
        /// </summary>
        public string DgpsStationId { get; }

        /// <summary>
        /// Navigational status
        /// </summary>
        public NavigationalStatus Status { get; }
	}
}
