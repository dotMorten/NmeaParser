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
    /// GNSS Satellite Fault Detection
    /// </summary>
    /// <remarks>
    /// <para>
    /// This sentence is used to support Receiver Autonomous Integrity Monitoring (RAIM). Given that a GNSS
    /// receiver is tracking enough satellites to perform integrity checks of the positioning quality of the position
    /// solution a sentence is needed to report the output of this process to other systems to advise the system
    /// user.With the RAIM in the GNSS receiver, the receiver can isolate faults to individual satellites and not
    /// use them in its position and velocity calculations.Also, the GNSS receiver can still track the satellite and
    /// easily judge when it is back within tolerance.This sentence shall be used for reporting this RAIM
    /// information. To perform this integrity function, the GNSS receiver must have at least two observables in
    /// addition to the minimum required for navigation.Normally these observables take the form of additional
    /// redundant satellites.
    /// </para>
    /// <para>
    /// If only GPS, GLONASS, Galileo, BDS, QZSS, NavIC (IRNSS) is used for the reported position solution
    /// the talker ID is GP, GL, GA, GB, GQ, GI respectively and the errors pertain to the individual system.If
    /// satellites from multiple systems are used to obtain the reported position solution the talker ID is GN and
    /// the errors pertain to the combined solution.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dtm")]
    [NmeaMessageType("--GBS")]
    public class Gbs : NmeaMessage, ITimestampedMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gbs"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Gbs (string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 8)
                throw new ArgumentException("Invalid GBS", "message");
            FixTime = StringToTimeSpan(message[0]);
            LatitudeError = NmeaMessage.StringToDouble(message[1]);
            LongitudeError = NmeaMessage.StringToDouble(message[2]);
            AltitudeError = NmeaMessage.StringToDouble(message[3]);
            if (int.TryParse(message[4], System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
                SatelliteId = id;
            MissedDetectionProbability = NmeaMessage.StringToDouble(message[5]);
            BiasEstimate = NmeaMessage.StringToDouble(message[6]);
            StandardDeviation = NmeaMessage.StringToDouble(message[7]);
        }
        /// <summary>
        /// UTC time of the GGA or GNS fix associated with this sentence.
        /// </summary>
        public TimeSpan FixTime { get; }

        TimeSpan ITimestampedMessage.Timestamp => FixTime;

        /// <summary>
        /// Expected Error in latitude
        /// </summary>
        /// <remarks>
        /// Expected error in meters due to bias, with noise = 0
        /// </remarks>
        public double LatitudeError { get; }

        /// <summary>
        /// Expected Error in longitude
        /// </summary>
        /// <remarks>
        /// Expected error in meters due to bias, with noise = 0
        /// </remarks>
        public double LongitudeError { get; }

        /// <summary>
        /// Expected Error in altitude
        /// </summary>
        /// <remarks>
        /// Expected error in meters due to bias, with noise = 0
        /// </remarks>
        public double AltitudeError { get; }

        /// <summary>
        /// ID number of most likely failed satellite
        /// </summary>
        /// <remarks>
        /// <para>
        /// Satellite ID numbers. To avoid possible confusion caused by repetition of satellite ID numbers when using
        /// multiple satellite systems, the following convention has been adopted: 
        /// <ul>
        /// <li>a) GPS satellites are identified by their PRN numbers, which range from 1 to 32.</li>
        /// <li>b) The numbers 33-64 are reserved for SBAS satellites. The SBAS system PRN numbers are 120-138.
        /// The offset from NMEA SBAS SV ID to SBAS PRN number is 87. A SBAS PRN number of 120
        /// minus 87 yields the SV ID of 33. The addition of 87 to the SV ID yields the SBAS PRN number.</li>
        /// <li>c) The numbers 65-96 are reserved for GLONASS satellites. GLONASS satellites are identified by
        /// 64+satellite slot number.The slot numbers are 1 through 24 for the full GLONASS constellation
        /// of 24 satellites, this gives a range of 65 through 88. The numbers 89 through 96 are available if
        /// slot numbers above 24 are allocated to on-orbit spares.
        /// </li>
        /// <li>See Note 3 for other GNSS not listed in a), b), or c) above to determine meaning of satellite ID when Talker ID GN is used</li>
        /// </ul>
        /// </para>
        /// 
        /// </remarks>
        public int? SatelliteId { get; }

        /// <summary>
        /// Probability of missed detection for most likely failed satellite
        /// </summary>
        public double MissedDetectionProbability { get; }

        /// <summary>
        /// Estimate of bias in meters on most likely failed satellite
        /// </summary>
        public double BiasEstimate { get; }

        /// <summary>
        /// Standard deviation of bias estimate
        /// </summary>
        public double StandardDeviation { get; }
    }
}