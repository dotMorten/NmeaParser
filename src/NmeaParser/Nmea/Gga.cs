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
    ///  Global Positioning System Fix Data
    /// </summary>
    /// <remarks>
    /// This sentence is designed for use only with the <c>GP</c> Talker ID for U.S. Global Positioning System. All Global Satellite System receivers,
    /// invcluding GPS, should use the <see cref="Gns"/> sentence in new equipment designs.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpgga")]
    [NmeaMessageType("--GGA")]
    public class Gga : NmeaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gga"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Gga(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 14)
                throw new ArgumentException("Invalid GPGGA", "message"); 
            FixTime = StringToTimeSpan(message[0]);
            Latitude = NmeaMessage.StringToLatitude(message[1], message[2]);
            Longitude = NmeaMessage.StringToLongitude(message[3], message[4]);
            Quality =  (Gga.FixQuality)int.Parse(message[5], CultureInfo.InvariantCulture);
            NumberOfSatellites = int.Parse(message[6], CultureInfo.InvariantCulture);
            Hdop = NmeaMessage.StringToDouble(message[7]);
            Altitude = NmeaMessage.StringToDouble(message[8]);
            AltitudeUnits = message[9];
            GeoidalSeparation = NmeaMessage.StringToDouble(message[10]);
            GeoidalSeparationUnits = message[11];            
            var timeInSeconds = StringToDouble(message[12]);
            if (!double.IsNaN(timeInSeconds))
                TimeSinceLastDgpsUpdate = TimeSpan.FromSeconds(timeInSeconds);
            else
                TimeSinceLastDgpsUpdate = TimeSpan.MaxValue;
            if (message[13].Length > 0)
                DgpsStationId = int.Parse(message[13], CultureInfo.InvariantCulture);
            else
                DgpsStationId = -1;
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
        /// Fix Quality
        /// </summary>
        public Gga.FixQuality Quality { get; }

        /// <summary>
        /// Number of satellites being tracked
        /// </summary>
        public int NumberOfSatellites { get; }

        /// <summary>
        /// Horizontal Dilution of Precision
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hdop")]
        public double Hdop { get; }

        /// <summary>
        /// Altitude
        /// </summary>
        public double Altitude { get; }

        /// <summary>
        /// Altitude units ('M' for Meters)
        /// </summary>
        public string AltitudeUnits { get; }
    
        /// <summary>
        /// Geoidal separation: the difference between the WGS-84 earth ellipsoid surface and mean-sea-level (geoid) surface.
        /// </summary>
        /// <remarks>
        /// A negative value means mean-sea-level surface is below the WGS-84 ellipsoid surface.
        /// </remarks>
        /// <seealso cref="GeoidalSeparationUnits"/>
        public double GeoidalSeparation { get; }

        /// <summary>
        /// Altitude units ('M' for Meters)
        /// </summary>
        public string GeoidalSeparationUnits { get; }

        /// <summary>
        /// Time since last DGPS update (ie age of the differential GPS data)
        /// </summary>
        public TimeSpan TimeSinceLastDgpsUpdate { get; }

        /// <summary>
        /// Differential Reference Station ID
        /// </summary>
        public int DgpsStationId { get; }

        /// <summary>
        /// Fix quality indicater
        /// </summary>
        public enum FixQuality : int
        {
            /// <summary>Fix not available or invalid</summary>
            Invalid = 0,
            /// <summary>GPS SPS Mode, fix valid</summary>
            GpsFix = 1,
            /// <summary>Differential GPS, SPS Mode, or Satellite Based Augmentation System (SBAS), fix valid</summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dgps")]
            DgpsFix = 2,
            /// <summary>GPS PPS (Precise Positioning Service) mode, fix valid</summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pps")]
            PpsFix = 3,
            /// <summary>Real Time Kinematic (Fixed). System used in RTK mode with fixed integers</summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rtk")]
            Rtk = 4,
            /// <summary>Real Time Kinematic (Floating). Satellite system used in RTK mode, floating integers</summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rtk")]
            FloatRtk = 5,
            /// <summary>Estimated (dead reckoning) mode</summary>
            Estimated = 6,
            /// <summary>Manual input mode</summary>
            ManualInput = 7,
            /// <summary>Simulator mode</summary>
            Simulation = 8
        }
    }
}
