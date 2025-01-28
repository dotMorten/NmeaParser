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
    ///     Water depth relative to the transducer, the depth offset of the transducer, and maximum depth that the sounder can detect a sea-bed (all in metres only).
    ///     Positive offsets provide distance from the transducer to the water line. Negative offsets provide distance from the transducer to the keel.
    ///     Not all NMEA 0183 devices that output this sentence can have their depth offset changed. In this case, the depth offset will always be zero, or not included.
    ///     NMEA 0183 v2.0 sentences will not include the maximum depth range value at all, as it was added in v3.0.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Format: $xxDPT,DATA_METRES,OFFSET_METRES, MAXIMUM_METRES*hh
    ///         1.: Data
    ///         2.: Offset
    ///         3.: Maximum
    ///     </para>
    ///     <para>
    ///         Depth
    ///     </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "SdDpt")]
    [NmeaMessageType("--DPT")]
    public class Dpt : NmeaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Dpt"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Dpt(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 3)
                throw new ArgumentException("Invalid Dpt", "message");

            DepthMeters = double.Parse(message[0], CultureInfo.InvariantCulture);
            DepthOffsetMeters = double.Parse(message[1], CultureInfo.InvariantCulture);
            MaxDepthRangeMeters = double.Parse(message[2], CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Depth, in meters
        /// </summary>
        public double DepthMeters { get; }

        /// <summary>
        ///     Depth offset, in meters
        /// </summary>
        public double DepthOffsetMeters { get; }

        /// <summary>
        ///     Maximum depth range, in meters
        /// </summary>
        public double MaxDepthRangeMeters { get; }
    }
}