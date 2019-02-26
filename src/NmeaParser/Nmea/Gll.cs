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
	///  Geographic position, latitude / longitude
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gll")]
    [NmeaMessageType("--GLL")]
    public class Gll : NmeaMessage
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="Gll"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Gll(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 4)
				throw new ArgumentException("Invalid GPGLL", "message");
			Latitude = NmeaMessage.StringToLatitude(message[0], message[1]);
			Longitude = NmeaMessage.StringToLongitude(message[2], message[3]);
			if (message.Length >= 5) //Some older GPS doesn't broadcast fix time
			{
				FixTime = StringToTimeSpan(message[4]);
			}
			DataActive = (message.Length < 6 || message[5] == "A");
		}

		/// <summary>
		/// Latitude
		/// </summary>
		public double Latitude { get; }

		/// <summary>
		/// Longitude
		/// </summary>
		public double Longitude { get; }

		/// <summary>
		/// Time since last DGPS update
		/// </summary>
		public TimeSpan FixTime { get; }

		/// <summary>
		/// Gets a value indicating whether data is active.
		/// </summary>
		/// <value>
		///   <c>true</c> if data is active; otherwise, <c>false</c>.
		/// </value>
		public bool DataActive { get; }

	}
}
