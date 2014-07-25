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
			Latitude = int.Parse(message[1].Substring(0, 2), CultureInfo.InvariantCulture) + double.Parse(message[1].Substring(2), CultureInfo.InvariantCulture) / 60;
			if (message[2] == "S")
				Latitude *= -1;
			Longitude = int.Parse(message[3].Substring(0, 3), CultureInfo.InvariantCulture) + double.Parse(message[3].Substring(3), CultureInfo.InvariantCulture) / 60;
			if (message[4] == "W")
				Latitude *= -1;
			Quality =  (FixQuality)int.Parse(message[5], CultureInfo.InvariantCulture);
			NumberOfSatellites = int.Parse(message[6], CultureInfo.InvariantCulture);
			Hdop = double.Parse(message[7], CultureInfo.InvariantCulture);
			Altitude = double.Parse(message[8], CultureInfo.InvariantCulture);
			AltitudeUnits = message[9];
			HeightOfGeoid = double.Parse(message[10], CultureInfo.InvariantCulture);
			HeightOfGeoidUnits = message[11];
			if (message[12].Length > 0)
				TimeSinceLastDgpsUpdate = TimeSpan.FromSeconds(int.Parse(message[12], CultureInfo.InvariantCulture));
			if (message[13].Length > 0)
				DgpsStationID = int.Parse(message[13], CultureInfo.InvariantCulture);
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
