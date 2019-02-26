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

namespace NmeaParser.Nmea
{
    /// <summary>
    /// Bearing Origin to Destination
    /// </summary>
    /// <remarks>
    /// Bearing angle of the line, calculated at the origin waypoint, extending to the destination waypoint from 
    /// the origin waypoint for the active navigation leg of the journey
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpbod")]
    [NmeaMessageType("--BOD")]
    public class Bod : NmeaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bod"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Bod(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 3)
                throw new ArgumentException("Invalid GPBOD", "message");
            if (message[0].Length > 0)
                TrueBearing = double.Parse(message[0], CultureInfo.InvariantCulture);
            else
                TrueBearing = double.NaN;
            if (message[2].Length > 0)
                MagneticBearing = double.Parse(message[2], CultureInfo.InvariantCulture);
            else
                MagneticBearing = double.NaN;
            if (message.Length > 4 && !string.IsNullOrEmpty(message[4]))
                DestinationId = message[4];
            if (message.Length > 5 && !string.IsNullOrEmpty(message[5]))
                OriginId = message[5];
        }
        /// <summary>
        /// True Bearing from start to destination
        /// </summary>
        public double TrueBearing { get; }

        /// <summary>
        /// Magnetic Bearing from start to destination
        /// </summary>
        public double MagneticBearing { get; }

        /// <summary>
        /// Name of origin
        /// </summary>
        public string OriginId { get; }

        /// <summary>
        /// Name of destination
        /// </summary>
        public string DestinationId { get; }
    }
}