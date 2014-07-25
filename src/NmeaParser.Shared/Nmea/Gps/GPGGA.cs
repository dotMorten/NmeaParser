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
	///  Global Positioning System Fix Data
	/// </summary>
	[NmeaMessageType(Type = "GPGGA")]
	public class Gpgga : NmeaMessage
	{
		public enum FixQuality : int
		{
			Invalid = 0,
			GpsFix = 1,
			DgpsFix = 2,
			PpsFix = 3,
			Rtk = 4,
			FloatRtk = 5,
			Estimated = 6,
			ManualInput = 7,
			Simulation = 8
		}

		protected override void LoadMessage(string[] message)
		{
			var time = message[0];
			Latitude = NmeaMessage.StringToLatitude(message[1], message[2]);
			Longitude = NmeaMessage.StringToLongitude(message[3], message[4]);
			Quality =  (FixQuality)int.Parse(message[5], CultureInfo.InvariantCulture);
			NumberOfSatellites = int.Parse(message[6], CultureInfo.InvariantCulture);
			Hdop = double.Parse(message[7], CultureInfo.InvariantCulture);
			Altitude = double.Parse(message[8], CultureInfo.InvariantCulture);
			AltitudeUnits = message[9];
			HeightOfGeoid = double.Parse(message[10], CultureInfo.InvariantCulture);
			HeightOfGeoidUnits = message[11];
			if (message[0].Length == 6)
			{
				TimeSinceLastDgpsUpdate = new TimeSpan(int.Parse(message[0].Substring(0, 2)),
								   int.Parse(message[0].Substring(2, 2)),
								   int.Parse(message[0].Substring(4, 2)));
			}
			if (message[13].Length > 0)
				DgpsStationID = int.Parse(message[13], CultureInfo.InvariantCulture);
			else
				DgpsStationID = -1;
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
		/// Fix Quality
		/// </summary>
		public FixQuality Quality { get; private set; }

		/// <summary>
		/// Number of satellites being tracked
		/// </summary>
		public int NumberOfSatellites { get; private set; }

		/// <summary>
		/// Horizontal Dilution of Precision
		/// </summary>
		public double Hdop { get; private set; }

		/// <summary>
		/// Altitude
		/// </summary>
		public double Altitude { get; private set; }

		/// <summary>
		/// Altitude units ('M' for Meters)
		/// </summary>
		public string AltitudeUnits { get; private set; }
	
		/// <summary>
		/// Height of geoid (mean sea level) above WGS84
		/// </summary>
		public double HeightOfGeoid { get; private set; }

		/// <summary>
		/// Altitude units ('M' for Meters)
		/// </summary>
		public string HeightOfGeoidUnits { get; private set; }

		/// <summary>
		/// Time since last DGPS update
		/// </summary>
		public TimeSpan TimeSinceLastDgpsUpdate { get; set; }

		/// <summary>
		/// DGPS Station ID Number
		/// </summary>
		public int DgpsStationID { get; set; }
	}
}
