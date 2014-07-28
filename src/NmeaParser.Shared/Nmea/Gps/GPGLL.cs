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
	///  Geographic position, latitude / longitude
	/// </summary>
	[NmeaMessageType(Type = "GPGLL")]
	public class Gpgll : NmeaMessage
	{
		protected override void LoadMessage(string[] message)
		{
			var time = message[0];
			Latitude = NmeaMessage.StringToLatitude(message[0], message[1]);
			Longitude = NmeaMessage.StringToLongitude(message[2], message[3]);
			if (message.Length >= 5 && message[4].Length == 6) //Some older GPS doesn't broadcast fix time
			{
				FixTime = new TimeSpan(int.Parse(message[4].Substring(0, 2)),
								   int.Parse(message[4].Substring(2, 2)),
								   int.Parse(message[4].Substring(4, 2)));
			}
			DataActive = (message.Length < 6 || message[5] == "A");
		}

		/// <summary>
		/// Latitude
		/// </summary>
		public double Latitude { get; private set; }

		/// <summary>
		/// Longitude
		/// </summary>
		public double Longitude { get; private set; }

		/// <summary>
		/// Time since last DGPS update
		/// </summary>
		public TimeSpan FixTime { get; set; }

		public bool DataActive { get; set; }

	}
}
