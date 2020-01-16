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
using System.Collections.Generic;
using System.Globalization;

namespace NmeaParser.Nmea
{
    /// <summary>
    /// GPS Satellites in view
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gsv")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [NmeaMessageType("--GSV")]
    public class Gsv : NmeaMultiSentenceMessage, IEnumerable<SatelliteVehicle>
    {
        private readonly List<SatelliteVehicle> svs = new List<SatelliteVehicle>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Gsv"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Gsv(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 3)
                throw new ArgumentException("Invalid GSV", "message");
        }

        /// <inheritdoc />
        protected override int MessageCountIndex => 0;

        /// <inheritdoc />
        protected override int MessageNumberIndex => 1;

        /// <inheritdoc />
        protected override bool ParseSentences(Talker talkerType, string[] message)
        {
            var satellites = int.Parse(message[2], CultureInfo.InvariantCulture);

            if (SVsInView == -1)
                SVsInView = satellites;
            else if ( satellites != SVsInView)
                return false; // Messages do not match

            for (int i = 3; i < message.Length - 3; i += 4)
            {
                if (message[i].Length == 0)
                    continue;
                else
                    svs.Add(new SatelliteVehicle(talkerType, message, i));
            }
            return true;
        }

        /// <summary>
        /// Total number of SVs in view
        /// </summary>
        public int SVsInView { get; private set; } = -1;

        /// <summary>
        /// Satellite vehicles in this message part.
        /// </summary>
        public IReadOnlyList<SatelliteVehicle> SVs => svs.AsReadOnly();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns> A System.Collections.Generic.IEnumerator{SatelliteVehicle} that can be used to iterate through the collection.</returns>
        public IEnumerator<SatelliteVehicle> GetEnumerator()
        {
            foreach (var sv in SVs)
                yield return sv;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns> An System.Collections.IEnumerator object that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    /// <summary>
    /// Satellite vehicle
    /// </summary>
    public sealed class SatelliteVehicle
    {
        internal SatelliteVehicle(Talker talker, string[] message, int startIndex)
        {
            PrnNumber = int.Parse(message[startIndex], CultureInfo.InvariantCulture);
            if (double.TryParse(message[startIndex + 1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double e))
                Elevation = e;
            if (double.TryParse(message[startIndex + 2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double a))
                Azimuth = a;
            int snr = -1;
            if (int.TryParse(message[startIndex + 3], out snr))
                SignalToNoiseRatio = snr;
            TalkerId = talker;
        }

        /// <summary>
        /// Gets the talker ID for this vehicle
        /// </summary>
        public Talker TalkerId { get; }

        /// <summary>
        /// SV PRN number
        /// </summary>
        public int PrnNumber { get; }

        /// <summary>
        /// Elevation in degrees, 90 maximum
        /// </summary>
        public double Elevation { get; } = double.NaN;

        /// <summary>
        /// Azimuth, degrees from true north, 000 to 359
        /// </summary>
        public double Azimuth { get; } = double.NaN;

        /// <summary>
        /// Signal-to-Noise ratio, 0-99 dB (-1 when not tracking) 
        /// </summary>
        public int SignalToNoiseRatio { get; } = -1;

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