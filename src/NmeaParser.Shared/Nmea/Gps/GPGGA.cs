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
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpgga")]
	[NmeaMessageType("GPGGA")]
	public class Gpgga : NmeaMessage
	{
		/// <summary>
		/// Fix quality
		/// </summary>
		public enum FixQuality : int
		{
			/// <summary>Invalid</summary>
			Invalid = 0,
			/// <summary>GPS</summary>
			GpsFix = 1,
			/// <summary>Differential GPS</summary>
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dgps")]
			DgpsFix = 2,
			/// <summary>Precise Positioning Service</summary>
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pps")]
			PpsFix = 3,
			/// <summary>Real Time Kinematic (Fixed)</summary>
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rtk")]
			Rtk = 4,
			/// <summary>Real Time Kinematic (Floating)</summary>
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rtk")]
			FloatRtk = 5,
			/// <summary>Estimated</summary>
			Estimated = 6,
			/// <summary>Manual input</summary>
			ManualInput = 7,
			/// <summary>Simulation</summary>
			Simulation = 8
		}

		/// <summary>
		/// Called when the message is being loaded.
		/// </summary>
		/// <param name="message">The NMEA message values.</param>
		protected override void OnLoadMessage(string[] message)
		{
			if (message == null || message.Length < 14)
				throw new ArgumentException("Invalid GPGGA", "message"); 
			Latitude = NmeaMessage.StringToLatitude(message[1], message[2]);
			Longitude = NmeaMessage.StringToLongitude(message[3], message[4]);
			Quality =  (FixQuality)int.Parse(message[5], CultureInfo.InvariantCulture);
			NumberOfSatellites = int.Parse(message[6], CultureInfo.InvariantCulture);
			Hdop = NmeaMessage.StringToDouble(message[7]);
			Altitude = NmeaMessage.StringToDouble(message[8]);
			AltitudeUnits = message[9];
			HeightOfGeoid = NmeaMessage.StringToDouble(message[10]);
			HeightOfGeoidUnits = message[11];
			if (message[0].Length == 6)
			{
				TimeSinceLastDgpsUpdate = new TimeSpan(int.Parse(message[0].Substring(0, 2), CultureInfo.InvariantCulture),
								   int.Parse(message[0].Substring(2, 2), CultureInfo.InvariantCulture),
								   int.Parse(message[0].Substring(4, 2), CultureInfo.InvariantCulture));
			}
			if (message[13].Length > 0)
				DgpsStationId = int.Parse(message[13], CultureInfo.InvariantCulture);
			else
				DgpsStationId = -1;
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hdop")]
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
		public int DgpsStationId { get; set; }
	}
}
