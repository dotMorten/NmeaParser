﻿//
// Copyright (c) 2015 Grzegorz Blok
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

namespace NmeaParser.Nmea.Base
{
    /// <summary>
    ///  GNSS (Global Navigation Satellite System) fix data
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gns")]
    public abstract class Gns : NmeaMessage
    {
        /// <summary>
        /// Mode indicator
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "Enum values matches NMEA spec")]
        public enum ModeIndicator
        {
            /// <summary>No fix available</summary>
            NoFix = 'N',
            /// <summary>Autonomous mode</summary>
            Autonomous = 'A',
            /// <summary>Differential mode</summary>
            Differential = 'D',
            /// <summary>Precise Positioning Service</summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pps")]
            PpsFix = 'P',
            /// <summary>Real Time Kinematic (Fixed)</summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rtk")]
            Rtk = 'R',
            /// <summary>Real Time Kinematic (Floating)</summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rtk")]
            FloatRtk = 'F',
            /// <summary>Estimated</summary>
            Estimated = 'E',
            /// <summary>Manual input</summary>
            ManualInput = 'M',
            /// <summary>Simulation</summary>
            Simulation = 'S'
        }

        /// <summary>
        /// Called when the message is being loaded.
        /// </summary>
        /// <param name="message">The NMEA message values.</param>
        protected override void OnLoadMessage(string[] message)
        {
            if (message == null || message.Length < 12)
                throw new ArgumentException("Invalid GNS", "message");

            if (message[0].Length >= 6)
            {
                FixTime = StringToTimeSpan(message[0]);
            }

            Latitude = NmeaMessage.StringToLatitude(message[1], message[2]);
            Longitude = NmeaMessage.StringToLongitude(message[3], message[4]);
            if (message[5].Length >= 2)
            {
                GpsMode = (ModeIndicator)message[5][0];
                GlonassMode = (ModeIndicator)message[5][1];
            }
            else
            {
                GpsMode = ModeIndicator.NoFix;
                GlonassMode = ModeIndicator.NoFix;
            }

            SVsInUse = int.Parse(message[6], CultureInfo.InvariantCulture);
            Hdop = NmeaMessage.StringToDouble(message[7]);
            HeightOfGeoid = NmeaMessage.StringToDouble(message[8]);
            GeoidalSeparation = NmeaMessage.StringToDouble(message[9]);
            var ageOfDiffDataSeconds = NmeaMessage.StringToDouble(message[10]);
            if (Double.IsNaN(ageOfDiffDataSeconds))
            {
                TimeSinceLastDiffDataUpdate = TimeSpan.Zero;
            }
            else
            {
                TimeSinceLastDiffDataUpdate = TimeSpan.FromSeconds(ageOfDiffDataSeconds);
            }

            if (message[11].Length > 0)
                DiffStationId = int.Parse(message[11], CultureInfo.InvariantCulture);
            else
                DiffStationId = -1;
        }

        /// <summary>
        /// GNSS source system
        /// </summary>
        public TalkerId Talker { get; protected set; }

        /// <summary>
        /// Fix Time
        /// </summary>
        public TimeSpan FixTime { get; protected set; }

        /// <summary>
        /// Latitude
        /// </summary>
        public double Latitude { get; protected set; }

        /// <summary>
        /// Longitude
        /// </summary>
        public double Longitude { get; protected set; }

        /// <summary>
        /// GPS mode indicator
        /// </summary>
        public ModeIndicator GpsMode { get; protected set; }

        /// <summary>
        /// GLONASS mode indicator
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Glonass")]
        public ModeIndicator GlonassMode { get; protected set; }

        /// <summary>
        /// Total number of satellites in use
        /// </summary>
        public int SVsInUse { get; protected set; }

        /// <summary>
        /// Horizontal Dilution of Precision
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hdop")]
        public double Hdop { get; protected set; }

        /// <summary>
        /// Antenna altitude, meters; Mean-sea-level (geoid)
        /// </summary>
        public double HeightOfGeoid { get; protected set; }

        /// <summary>
        /// The difference between the WGS-84 earth ellipsoid surface and mean-sea-level (geoid) surface;
        /// mean-sea-level surface below ellipsoid. In meters.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Geoidal")]
        public double GeoidalSeparation { get; protected set; }

        /// <summary>
        /// Age of differential data
        /// </summary>
        public TimeSpan TimeSinceLastDiffDataUpdate { get; protected set; }

        /// <summary>
        /// Differential Station ID Number
        /// </summary>
        public int DiffStationId { get; protected set; }
    }
}
