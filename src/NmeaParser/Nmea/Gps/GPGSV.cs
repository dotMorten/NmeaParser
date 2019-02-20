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

namespace NmeaParser.Nmea.Gps
{
    /// <summary>
    ///  GLONASS Satellites in view
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpgsv")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [NmeaMessageType("GPGSV")]
    public sealed class Gpgsv : Gsv
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gpgst"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Gpgsv(string type, string[] message) : base(type, message) { }
    }

    /// <summary>
    /// Satellite vehicle
    /// </summary>
    public sealed class SatelliteVehicle
    {
        internal SatelliteVehicle(string[] message, int startIndex)
        {
            PrnNumber = int.Parse(message[startIndex], CultureInfo.InvariantCulture);
            Elevation = double.Parse(message[startIndex + 1], CultureInfo.InvariantCulture);
            Azimuth = double.Parse(message[startIndex + 2], CultureInfo.InvariantCulture);
            int snr = -1;
            if (int.TryParse(message[startIndex + 3], out snr))
                SignalToNoiseRatio = snr;
        }
        /// <summary>
        /// SV PRN number
        /// </summary>
        public int PrnNumber { get; }
        /// <summary>
        /// Elevation in degrees, 90 maximum
        /// </summary>
        public double Elevation{ get; }
        /// <summary>
        /// Azimuth, degrees from true north, 000 to 359
        /// </summary>
        public double Azimuth{ get; }
        /// <summary>
        /// Signal-to-Noise ratio, 0-99 dB (-1 when not tracking) 
        /// </summary>
        public int SignalToNoiseRatio{ get; }

        /// <summary>
        /// Satellite system
        /// </summary>
        public SatelliteSystem System
        {
            get
            {
                if (PrnNumber >= 1 && PrnNumber <= 32)
                    return SatelliteSystem.Gps;
                if (PrnNumber >= 33 && PrnNumber <= 64)
                    return SatelliteSystem.Waas;
                if (PrnNumber >= 65 && PrnNumber <= 96)
                    return SatelliteSystem.Glonass;
                return SatelliteSystem.Unknown;
            }
        }
    }

    /// <summary>
    /// Satellite system
    /// </summary>
    public enum SatelliteSystem
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// GPS - Global Positioning System (NAVSTAR)
        /// </summary>
        Gps,
        /// <summary>
        /// WAAS - Wide Area Augmentation System
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Waas")]
        Waas,
        /// <summary>
        /// GLONASS - Globalnaya navigatsionnaya sputnikovaya sistema
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Glonass")]
        Glonass,
        /// <summary>
        /// Galileo
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Galileo")]
        Galileo
    }
}