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
using System.Threading;
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
            FixTime = new DateTime(int.Parse(message[8].Substring(4, 2), CultureInfo.InvariantCulture) + 2000,
                                    int.Parse(message[8].Substring(2, 2), CultureInfo.InvariantCulture),
                                    int.Parse(message[8].Substring(0, 2), CultureInfo.InvariantCulture),
                                    int.Parse(message[0].Substring(0, 2), CultureInfo.InvariantCulture),
                                    int.Parse(message[0].Substring(2, 2), CultureInfo.InvariantCulture),
                                    int.Parse(message[0].Substring(4, 2), CultureInfo.InvariantCulture), DateTimeKind.Utc);
            Active = (message[1] == "A");
            Latitude = NmeaMessage.StringToLatitude(message[2], message[3]);
            Longitude = NmeaMessage.StringToLongitude(message[4], message[5]);
            Speed = !string.IsNullOrWhiteSpace(message[6]) ? double.Parse(message[6], CultureInfo.InvariantCulture) : double.NaN;
            Course = !string.IsNullOrWhiteSpace(message[7]) ? double.Parse(message[7], CultureInfo.InvariantCulture) : double.NaN;
            MagneticVariation = !string.IsNullOrWhiteSpace(message[9]) ? double.Parse(message[9], CultureInfo.InvariantCulture) : double.NaN;
            if (message[10] == "W")
                MagneticVariation *= -1;
		}

		/// <summary>
		/// Fix Time
		/// </summary>
		public DateTime FixTime { get; set; }

		/// <summary>
		/// Gets a value whether the device is active
		/// </summary>
		public bool Active { get; set; }

		/// <summary>
		/// Latitude
		/// </summary>
		public double Latitude { get; set; }

		/// <summary>
		/// Longitude
		/// </summary>
		public double Longitude { get; set; }

		/// <summary>
		/// Speed over the ground in knots
		/// </summary>
		public double Speed { get; set; }

		/// <summary>
		/// Track angle in degrees True
		/// </summary>
		public double Course { get; set; }

		/// <summary>
		/// Magnetic Variation
		/// </summary>
		public double MagneticVariation { get; set; }
	}
}
