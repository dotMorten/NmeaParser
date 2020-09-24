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
using System.Linq;

namespace NmeaParser.Messages
{
    /// <summary>
    /// GNSS Satellites in view
    /// </summary>
    /// <remarks>
    /// The GSV sentence provides the number of satellites (SV) in view, satellite ID numbers, elevation, azimuth, and SNR value.
    /// </remarks>
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

            if (SatellitesInView == -1)
                SatellitesInView = satellites;
            else if ( satellites != SatellitesInView)
                return false; // Messages do not match

            if ((message.Length - 3) % 4 == 1) // v4.1+ adds system id to the last message. Example L1=1, and L2=6 on GPS satellites
            {
                var id = message.Last();
                if (id.Length == 1)
                {
                    GnssSignalId = id[0];
                }
            }
            for (int i = 3; i < message.Length - 3; i += 4)
            {
                if (message[i].Length == 0)
                    continue;
                else
                    svs.Add(new SatelliteVehicle(talkerType, GnssSignalId, message, i));
            }
            return true;
        }

        /// <summary>
        /// Total number of satellite vehicles (SV) in view
        /// </summary>
        /// <seealso cref="SVs"/>
        public int SatellitesInView { get; private set; } = -1;

        /// <summary>
        /// Satellite vehicles in this message part.
        /// </summary>
        public IReadOnlyList<SatelliteVehicle> SVs => svs.AsReadOnly();

        /// <summary>
        /// Gets the GNSS Signal ID
        /// </summary>
        /// <remarks>
        /// <table>
        ///  <thead>
        ///    <tr><th>System</th><th>Signal ID</th><th>Signal Channel</th></tr>
        ///  </thead>
        ///  <tbody>
        ///    <tr><td>GPS</td><td>0</td><td>All signals</td></tr>
        ///    <tr><td></td><td>1</td><td>L1 C/1</td></tr>
        ///    <tr><td></td><td>2</td><td>L1 P(Y)</td></tr>
        ///    <tr><td></td><td>3</td><td>L1 M</td></tr>
        ///    <tr><td></td><td>4</td><td>L2 P(Y)</td></tr>
        ///    <tr><td></td><td>5</td><td>L2C-M</td></tr>
        ///    <tr><td></td><td>6</td><td>L2C-L</td></tr>
        ///    <tr><td></td><td>7</td><td>L5-I</td></tr>
        ///    <tr><td></td><td>8</td><td>L5-Q</td></tr>
        ///    <tr><td></td><td>9-F</td><td>Reserved</td></tr>
        ///    <tr><td>GLONASS</td><td>0</td><td>All signals</td></tr>
        ///    <tr><td></td><td>1</td><td>G1 C/A</td></tr>
        ///    <tr><td></td><td>2</td><td>G1 P</td></tr>
        ///    <tr><td></td><td>3</td><td>G2 C/A</td></tr>
        ///    <tr><td></td><td>4</td><td>GLONASS (M) G2 P</td></tr>
        ///    <tr><td></td><td>5-F</td><td>Reserved</td></tr>
        ///    <tr><td>GALILEO</td><td>0</td><td>All signals</td></tr>
        ///    <tr><td></td><td>1</td><td>E5a</td></tr>
        ///    <tr><td></td><td>2</td><td>E5b</td></tr>
        ///    <tr><td></td><td>3</td><td>E5 a+b</td></tr>
        ///    <tr><td></td><td>4</td><td>E6-A</td></tr>
        ///    <tr><td></td><td>5</td><td>E6-BC</td></tr>
        ///    <tr><td></td><td>6</td><td>L1-A</td></tr>
        ///    <tr><td></td><td>7</td><td>L1-BC</td></tr>
        ///    <tr><td></td><td>8-F</td><td>Reserved</td></tr>
        ///    <tr><td>BeiDou System</td><td>0</td><td>All signals</td></tr>
        ///    <tr><td></td><td>1</td><td>B1I</td></tr>
        ///    <tr><td></td><td>2</td><td>B1Q</td></tr>
        ///    <tr><td></td><td>3</td><td>B1C</td></tr>
        ///    <tr><td></td><td>4</td><td>B1A</td></tr>
        ///    <tr><td></td><td>5</td><td>B2-a</td></tr>
        ///    <tr><td></td><td>6</td><td>B2-b</td></tr>
        ///    <tr><td></td><td>7</td><td>B2 a+b</td></tr>
        ///    <tr><td></td><td>8</td><td>B3I</td></tr>
        ///    <tr><td></td><td>9</td><td>B3Q</td></tr>
        ///    <tr><td></td><td>A</td><td>B3A</td></tr>
        ///    <tr><td></td><td>B</td><td>B2I</td></tr>
        ///    <tr><td></td><td>C</td><td>B2Q</td></tr>
        ///    <tr><td></td><td>D-F</td><td>Reserved</td></tr>
        ///    <tr><td>QZSS</td><td>0</td><td>All signals</td></tr>
        ///    <tr><td></td><td>1</td><td>L1 C/A</td></tr>
        ///    <tr><td></td><td>2</td><td>L1C (D)</td></tr>
        ///    <tr><td></td><td>3</td><td>L1C (P)</td></tr>
        ///    <tr><td></td><td>4</td><td>LIS</td></tr>
        ///    <tr><td></td><td>5</td><td>L2C-M</td></tr>
        ///    <tr><td></td><td>6</td><td>L2C-L</td></tr>
        ///    <tr><td></td><td>7</td><td>L5-I</td></tr>
        ///    <tr><td></td><td>8</td><td>L5-Q</td></tr>
        ///    <tr><td></td><td>9</td><td>L6D</td></tr>
        ///    <tr><td></td><td>A</td><td>L6E</td></tr>
        ///    <tr><td></td><td>B-F</td><td>Reserved</td></tr>
        ///    <tr><td>NavIC (IRNSS)</td><td>0</td><td>All signals</td></tr>
        ///    <tr><td></td><td>1</td><td>L5-SPS</td></tr>
        ///    <tr><td></td><td>2</td><td>S-SPS</td></tr>
        ///    <tr><td></td><td>3</td><td>L5-RS</td></tr>
        ///    <tr><td></td><td>4</td><td>S-RS</td></tr>
        ///    <tr><td></td><td>5</td><td>L1-SPS</td></tr>
        ///    <tr><td></td><td>6-F</td><td>Reserved</td></tr>
        ///  </tbody>
        ///</table>
        /// </remarks>
        public char GnssSignalId { get; private set; } = '0';

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
        internal SatelliteVehicle(Talker talker, char signalId, string[] message, int startIndex)
        {
            Id = int.Parse(message[startIndex], CultureInfo.InvariantCulture);
            if (double.TryParse(message[startIndex + 1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double e))
                Elevation = e;
            if (double.TryParse(message[startIndex + 2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double a))
                Azimuth = a;
            int snr = -1;
            if (int.TryParse(message[startIndex + 3], out snr))
                SignalToNoiseRatio = snr;
            GnssSignalId = signalId;
            TalkerId = talker;
        }

        /// <summary>
        /// Gets the talker ID for this vehicle
        /// </summary>
        public Talker TalkerId { get; }

        /// <summary>
        /// Gets the GNSS Signal ID.
        /// </summary>
        /// <seealso cref="Gsv.GnssSignalId"/>
        public char GnssSignalId { get; }

        /// <summary>
        /// Satellite ID number
        /// </summary>
        public int Id { get; }

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
                if (Id >= 1 && Id <= 32)
                    return SatelliteSystem.Gps;
                if (Id >= 33 && Id <= 64)
                    return SatelliteSystem.Waas;
                if (Id >= 65 && Id <= 96)
                    return SatelliteSystem.Glonass;
                return SatelliteSystem.Unknown;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            switch (TalkerId)
            {
                case Talker.GlobalPositioningSystem: return $"GPS{Id}";
                case Talker.GlonassReceiver: return $"GLO{Id}";
                case Talker.GalileoPositioningSystem: return $"GAL{Id}";
                case Talker.BeiDouNavigationSatelliteSystem: return $"BEI{Id}";
                default: return Id.ToString();
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