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

namespace NmeaParser.Nmea.Gps
{
	/// <summary>
	/// Bearing Origin to Destination
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpbod")]
	[NmeaMessageType("GPBOD")]
	public class Gpbod : NmeaMessage
	{
		/// <summary>
		/// Called when the message is being loaded.
		/// </summary>
		/// <param name="message">The NMEA message values.</param>
		protected override void OnLoadMessage(string[] message)
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
		public double TrueBearing { get; private set; }

		/// <summary>
		/// Magnetic Bearing from start to destination
		/// </summary>
		public double MagneticBearing { get; private set; }

		/// <summary>
		/// Name of origin
		/// </summary>
		public string OriginId { get; private set; }

		/// <summary>
		/// Name of destination
		/// </summary>
		public string DestinationId { get; private set; }
	}
}
