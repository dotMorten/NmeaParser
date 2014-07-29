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
	///  GPS Satellites in view
	/// </summary>
	[NmeaMessageType(Type = "GPGSV")]
	public sealed class Gpgsv : NmeaMessage
	{
		protected override void LoadMessage(string[] message)
		{
			TotalMessages = int.Parse(message[0]);
			MessageNumber = int.Parse(message[1]);
			SVsInView = int.Parse(message[2]);

			List<SatelitteVehicle> svs = new List<SatelitteVehicle>();
			for (int i = 3; i < 18; i+=4)
			{
				if (message[i].Length == 0)
					continue;
				else
					svs.Add(new SatelitteVehicle(message, i));
			}
			this.SVs = svs.ToArray();
		}

		/// <summary>
		/// Total number of messages of this type in this cycle
		/// </summary>
		public int TotalMessages { get; private set; }

		/// <summary>
		/// Message number
		/// </summary>
		public int MessageNumber { get; private set; }

		/// <summary>
		/// Total number of SVs in view
		/// </summary>
		public int SVsInView { get; private set; }

		/// <summary>
		/// Dilution of precision
		/// </summary>
		public SatelitteVehicle[] SVs { get; private set; }
	}

	public sealed class SatelitteVehicle
	{
		internal SatelitteVehicle(string[] message, int startIndex)
		{
			PrnNumber = int.Parse(message[startIndex]);
			Elevation = double.Parse(message[startIndex+1], CultureInfo.InvariantCulture);
			Azimuth = double.Parse(message[startIndex + 2], CultureInfo.InvariantCulture);
			int snr = -1;
			if (int.TryParse(message[startIndex + 3], out snr))
				SignalToNoiseRatio = snr;
		}
		/// <summary>
		/// SV PRN number
		/// </summary>
		public int PrnNumber { get; set; }
		/// <summary>
		/// Elevation in degrees, 90 maximum
		/// </summary>
		public double Elevation { get; private set; }
		/// <summary>
		/// Azimuth, degrees from true north, 000 to 359
		/// </summary>
		public double Azimuth { get; private set; }
		/// <summary>
		/// Signal-to-Noise ratio, 0-99 dB (-1 when not tracking) 
		/// </summary>
		public int SignalToNoiseRatio { get; private set; }
	}
}
