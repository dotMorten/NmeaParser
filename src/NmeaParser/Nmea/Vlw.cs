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

namespace NmeaParser.Messages
{
    /// <summary>
    /// Dual Ground/Water Distance
    /// </summary>
    /// <remarks>
    /// The distance traveled, relative to the water and over the ground.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dtm")]
    [NmeaMessageType("--VLW")]
    public class Vlw : NmeaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vlw"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Vlw (string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 7)
                throw new ArgumentException("Invalid VLW", "message");
            WaterDistanceCumulative = NmeaMessage.StringToDouble(message[0]);
            WaterDistanceSinceReset = NmeaMessage.StringToDouble(message[2]);
            GroundDistanceCumulative = NmeaMessage.StringToDouble(message[4]);
            GroundDistanceSinceReset = NmeaMessage.StringToDouble(message[6]);
        }
		
		/// <summary>Total cumulative water distance, nautical miles</summary>
        public double WaterDistanceCumulative { get; }
		
		/// <summary>Water distance since reset, nautical miles</summary>
        public double WaterDistanceSinceReset { get; }
		
		/// <summary>Total cumulative ground distance, nautical miles</summary>
        public double GroundDistanceCumulative { get; }
		
		/// <summary>Ground distance since reset, nautical miles</summary>
        public double GroundDistanceSinceReset { get; }
    }
}