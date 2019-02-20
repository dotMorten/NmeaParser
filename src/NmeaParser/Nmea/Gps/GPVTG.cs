//
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
using System.Text;

namespace NmeaParser.Nmea.Gps
{
    /// <summary>
	///  Course over ground and ground speed
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "GPVTG")]
    [NmeaMessageType("GPVTG")]
    public class Gpvtg : NmeaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gpvtg"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Gpvtg(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 7)
                throw new ArgumentException("Invalid Gpvtg", "message");
            TrueCourseOverGround = NmeaMessage.StringToDouble(message[0]);
            MagneticCourseOverGround = NmeaMessage.StringToDouble(message[2]);
            SpeedInKnots = NmeaMessage.StringToDouble(message[4]);
            SpeedInKph = NmeaMessage.StringToDouble(message[6]);
        }

        /// <summary>
        ///  Course over ground relative to true north
        /// </summary>
        public double TrueCourseOverGround { get; }

        /// <summary>
        ///  Course over ground relative to magnetic north
        /// </summary>
        public double MagneticCourseOverGround { get; }

        /// <summary>
        /// Speed over ground in knots
        /// </summary>
        public double SpeedInKnots { get; }

        /// <summary>
        /// Speed over ground in kilometers/hour
        /// </summary>
        public double SpeedInKph { get; }
    }
}