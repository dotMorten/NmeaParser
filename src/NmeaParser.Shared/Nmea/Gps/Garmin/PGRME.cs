using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser.Nmea.Gps.Garmin
{
	/// <summary>
	///  Recommended Minimum
	/// </summary>
	[NmeaMessageType(Type = "PGRME")]
	public class Pgrme : NmeaMessage
	{
		protected override void LoadMessage(string[] message)
		{
			HorizontalError = double.Parse(message[0], CultureInfo.InvariantCulture);
			HorizontalErrorUnits = message[1];
			VerticalError = double.Parse(message[2], CultureInfo.InvariantCulture);
			VerticalErrorUnits = message[3];
			SphericalError = double.Parse(message[4], CultureInfo.InvariantCulture);
			SphericalErrorUnits = message[5];
		}

		/// <summary>
		/// Estimated horizontal position error in meters (HPE)
		/// </summary>
		public double HorizontalError { get; private set; }

		/// <summary>
		/// Horizontal Error unit ('M' for Meters)
		/// </summary>
		public string HorizontalErrorUnits { get; private set; }

		/// <summary>
		/// Estimated vertical position error in meters (VPE)
		/// </summary>
		public double VerticalError { get; private set; }

		/// <summary>
		/// Vertical Error unit ('M' for Meters)
		/// </summary>
		public string VerticalErrorUnits { get; private set; }

		/// <summary>
		/// Overall spherical equivalent position error
		/// </summary>
		public double SphericalError { get; private set; }

		/// <summary>
		/// Spherical Error unit ('M' for Meters)
		/// </summary>
		public string SphericalErrorUnits { get; private set; }
	}
}
