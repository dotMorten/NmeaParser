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
	///  Recommended Minimum
	/// </summary>
	[NmeaMessageType(Type = "GPRMC")]
	public class Gprmc : NmeaMessage
	{
		protected override void LoadMessage(string[] message)
		{
			if (message[8].Length == 6 && message[0].Length == 6)
			{
				FixTime = new DateTime(int.Parse(message[8].Substring(4, 2)) + 2000,
									   int.Parse(message[8].Substring(2, 2)),
									   int.Parse(message[8].Substring(0, 2)),
									   int.Parse(message[0].Substring(0, 2)),
									   int.Parse(message[0].Substring(2, 2)),
									   int.Parse(message[0].Substring(4, 2)), DateTimeKind.Utc);
			}
			Active = (message[1] == "A");
			Latitude = NmeaMessage.StringToLatitude(message[2], message[3]);
			Longitude = NmeaMessage.StringToLongitude(message[4], message[5]);
			Speed = NmeaMessage.StringToDouble(message[6]);
			Course = NmeaMessage.StringToDouble(message[7]);
			MagneticVariation = NmeaMessage.StringToDouble(message[9]);			
			if (!double.IsNaN(MagneticVariation) && message[10] == "W")
				MagneticVariation *= -1;
		}

		/// <summary>
		/// Fix Time
		/// </summary>
		public DateTime FixTime { get; private set; }

		/// <summary>
		/// Gets a value whether the device is active
		/// </summary>
		public bool Active { get; private set; }

		/// <summary>
		/// Latitude
		/// </summary>
		public double Latitude { get; private set; }

		/// <summary>
		/// Longitude
		/// </summary>
		public double Longitude { get; private set; }

		/// <summary>
		/// Speed over the ground in knots
		/// </summary>
		public double Speed { get; private set; }

		/// <summary>
		/// Track angle in degrees True
		/// </summary>
		public double Course { get; private set; }

		/// <summary>
		/// Magnetic Variation
		/// </summary>
		public double MagneticVariation { get; private set; }
	}
}
