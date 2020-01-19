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

namespace NmeaParser.Nmea
{
    /// <summary>
    /// Laser Range Measurement
    /// </summary>
    public abstract class LaserRangeMessage : NmeaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LaserRangeMessage"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        protected LaserRangeMessage(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 9)
                throw new ArgumentException("Invalid Laser Range Message", "message"); 
            
            HorizontalVector = message[0];
            HorizontalDistance = double.Parse(message[1], CultureInfo.InvariantCulture);
            HorizontalDistanceUnits = message[2][0];
            HorizontalAngle = double.Parse(message[3], CultureInfo.InvariantCulture);
            HorizontalAngleUnits = message[4][0];
            VerticalAngle = double.Parse(message[5], CultureInfo.InvariantCulture);
            VerticalAngleUnits = message[6][0];
            SlopeDistance = double.Parse(message[7], CultureInfo.InvariantCulture);
            SlopeDistanceUnits = message[8][0];
        }

        /// <summary>
        /// Gets the horizontal vector.
        /// </summary>
        public string HorizontalVector { get; }

        /// <summary>
        /// Gets the horizontal distance.
        /// </summary>
        public double HorizontalDistance { get; }

        /// <summary>
        /// Gets the units of the <see cref="HorizontalDistance"/> value.
        /// </summary>
        public char HorizontalDistanceUnits { get; }

        /// <summary>
        /// Gets the horizontal angle.
        /// </summary>
        public double HorizontalAngle { get; }

        /// <summary>
        /// Gets the units of the <see cref="HorizontalAngle"/> value.
        /// </summary>
        public char HorizontalAngleUnits { get; }

        /// <summary>
        /// Gets the vertical angle.
        /// </summary>
        public double VerticalAngle { get; }

        /// <summary>
        /// Gets the units of the <see cref="VerticalAngle"/> value.
        /// </summary>
        public char VerticalAngleUnits { get; }

        /// <summary>
        /// Gets the slope distance.
        /// </summary>
        public double SlopeDistance { get; }

        /// <summary>
        /// Gets the units of the <see cref="SlopeDistance"/> value.
        /// </summary>
        public char SlopeDistanceUnits { get; }
    }
}
