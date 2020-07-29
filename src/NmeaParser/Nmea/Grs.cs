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

namespace NmeaParser.Messages
{
    /// <summary>
    /// GNSS Range Residuals
    /// </summary>
    /// <remarks>
    /// <para>
    /// This sentence is used to support Receiver Autonomous Integrity Monitoring (RAIM). Range residuals can be
    /// computed in two ways for this process. The basic measurement integration cycle of most navigation filters
    /// generates a set of residuals and uses these to update the position state of the receiver.
    /// </para>
    /// <para>
    /// These residuals can be reported with GRS, but because of the fact that these were used to generate the navigation
    /// solution they should be recomputed using the new solution in order to reflect the residuals for the position solution in
    /// the GGA or GNS sentence.
    /// </para>
    /// <para>
    /// The MODE field should indicate which computation method was used. An integrity process that uses these
    /// range residuals would also require GGA or GNS, GSA, and GSV sentences to be sent.
    /// </para>
    /// <para>
    /// If only GPS, or GLONASS, or Galileo, or BDS, or QZSS, or NavIC (IRNSS)is used for the reported position
    /// solution, the talker ID is GP, GL, GA, GB, GQ, GI respectively and the range residuals pertain to the individual
    /// system.
    /// </para>
    /// <para>
    /// If GPS, GLONASS, Galileo, BDS, QZSS, NavIC (IRNSS) are combined to obtain the position solution multiple
    /// GRS sentences are produced, one with the GPS satellites, another with the GLONASS satellites, etc. Each of these
    /// GRS sentences shall have talker ID “GN”, to indicate that the satellites are used in a combined solution. The GNSS
    /// System ID data field identifies the specific satellite system. It is important to distinguish the residuals from those that
    /// would be produced by a GPS-only, GLONASS-only, etc. position solution. In general, the residuals for a combined
    /// solution will be different from the residual for a GPS-only, GLONASS-only, etc. solution.
    /// </para>
    /// <para>
    /// When multiple GRS sentences are necessary, use of the NMEA TAG Block structure (§ 7) and the TAG Block
    /// Sentence-grouping Parameter (§ 7.9.3) reliably links the related sentences together over any transport medium.
    /// </para>
    /// <para>
    /// When GRS sentences are provided with related GSA and/or GSV sentences, use of the NMEA TAG Block structure
    /// (§ 7) and the TAG Block Sentence-grouping Parameter (§ 7.9.3) reliably links the related (different sentence
    /// formatters) sentences together over any transport medium.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dtm")]
    [NmeaMessageType("--GRS")]
    public class Grs : NmeaMessage, ITimestampedMessage
    {
        /// <summary>
        /// Determines the way the <see cref="Grs"/> residuals were calculated.
        /// </summary>
        public enum GrsMode
        {
            /// <summary>
            /// Residuals were used to calculate the position given in the matching GGA or GNS sentence
            /// </summary>
            UsedForPosition,
            /// <summary>
            /// Residuals were recomputed after the GGA or GNS position was computed
            /// </summary>
            RecomputedFromPosition
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Gbs"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Grs (string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 8)
                throw new ArgumentException("Invalid Grs", "message");
            FixTime = StringToTimeSpan(message[0]);
            Mode = message[1] == "1" ? GrsMode.RecomputedFromPosition : GrsMode.UsedForPosition;
            double[] residuals = new double[message.Length - 2];
            for (int i = 2; i < message.Length; i++)
            {
                residuals[i-2] = NmeaMessage.StringToDouble(message[i]);
            }
            Residuals = residuals;
        }

        /// <summary>
        /// UTC time of the GGA or GNS fix associated with this sentence 
        /// </summary>
        public TimeSpan FixTime { get; }

        /// <summary>
        /// Residual calculation mode
        /// </summary>
        public GrsMode Mode { get; }

        TimeSpan ITimestampedMessage.Timestamp => FixTime;

        /// <summary>
        /// Range residuals in meters for satellites used in the navigation solution
        /// </summary>
        /// <remarks>
        /// <para>
        /// Order must match order of the satellite ID3 numbers in GSA. When GRS is used GSA and GSV are generally required
        /// </para>
        /// <para>
        /// Notes:
        /// <ul>
        /// <li>If the range residual exceeds +99.9 meters, then the decimal part is dropped, resulting in an integer (-103.7 becomes -103).
        /// The maximum value for this field is +999.</li>
        /// <li>The sense or sign of the range residual is determined by the order of parameters used in the calculation. The
        /// expected order is as follows: range residual = calculated range - measured range.</li>
        /// <li>When multiple GRS sentences are being sent then their order of transmission must match the order of
        /// corresponding GSA sentences.Listeners shall keep track of pairs of GSA and GRS sentences and discard data
        /// if pairs are incomplete.</li>
        /// </ul>
        /// </para>
        /// </remarks>
        public double[] Residuals { get; }
    }
}