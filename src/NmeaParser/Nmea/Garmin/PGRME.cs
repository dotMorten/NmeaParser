﻿//  *******************************************************************************
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

namespace NmeaParser.Nmea.Garmin
{
    /// <summary>
    ///  Recommended Minimum
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pgrme")]
    [NmeaMessageType("PGRME")]
    public class Pgrme : NmeaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pgrme"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Pgrme(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 6)
                throw new ArgumentException("Invalid PGRME", "message"); 
            
            HorizontalError = NmeaMessage.StringToDouble(message[0]);
            HorizontalErrorUnits = message[1];
            VerticalError = NmeaMessage.StringToDouble(message[2]);
            VerticalErrorUnits = message[3];
            SphericalError = NmeaMessage.StringToDouble(message[4]);
            SphericalErrorUnits = message[5];
        }

        /// <summary>
        /// Estimated horizontal position error in meters (HPE)
        /// </summary>
        /// <remarks>Range: 0.0 to 999.9 meters</remarks>
        public double HorizontalError{ get; }

        /// <summary>
        /// Horizontal Error unit ('M' for Meters)
        /// </summary>
        public string HorizontalErrorUnits{ get; }

        /// <summary>
        /// Estimated vertical position error in meters (VPE)
        /// </summary>
        /// <remarks>Range: 0.0 to 999.9 meters</remarks>
        public double VerticalError{ get; }

        /// <summary>
        /// Vertical Error unit ('M' for Meters)
        /// </summary>
        public string VerticalErrorUnits{ get; }

        /// <summary>
        /// Overall spherical equivalent position error (EPE)
        /// </summary>
        /// <remarks>Range: 0.0 to 999.9 meters</remarks>
        public double SphericalError{ get; }

        /// <summary>
        /// Spherical Error unit ('M' for Meters)
        /// </summary>
        public string SphericalErrorUnits{ get; }
    }
}
