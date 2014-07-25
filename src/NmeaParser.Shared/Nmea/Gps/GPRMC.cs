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
			FixTime = new DateTime(int.Parse(message[8].Substring(4, 2)) + 2000,
								   int.Parse(message[8].Substring(2, 2)),
								   int.Parse(message[8].Substring(0, 2)),
								   int.Parse(message[0].Substring(0, 2)),
								   int.Parse(message[0].Substring(2, 2)),
								   int.Parse(message[0].Substring(4, 2)), DateTimeKind.Utc);
			Active = (message[1] == "A");
			Latitude = int.Parse(message[2].Substring(0, 2), CultureInfo.InvariantCulture) + double.Parse(message[2].Substring(2), CultureInfo.InvariantCulture) / 60;
			if (message[3] == "S")
				Latitude *= -1;
			Longitude = int.Parse(message[4].Substring(0, 3), CultureInfo.InvariantCulture) + double.Parse(message[4].Substring(3), CultureInfo.InvariantCulture) / 60;
			if (message[5] == "W")
				Longitude *= -1;
			Speed = double.Parse(message[6], CultureInfo.InvariantCulture);
			Course = double.Parse(message[7], CultureInfo.InvariantCulture);
			MagneticVariation = double.Parse(message[9], CultureInfo.InvariantCulture);
			if (message[10] == "W")
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
