using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTDevices.Nmea
{
	/// <summary>
	/// Laser Range Measurement
	/// </summary>
	public abstract class LaserRangeMessage : NmeaMessage
	{
		protected override void LoadMessage(string[] message)
		{
			HorizontalVector = message[0];
			HorizontalDistance = double.Parse(message[1], CultureInfo.InvariantCulture);
			HorizontalDistanceUnits = message[2][0];
			HorizontalAngle = double.Parse(message[3], CultureInfo.InvariantCulture);
			HorizontalAngleUnits = message[4][0];
			VerticalAngle = double.Parse(message[5], CultureInfo.InvariantCulture);
			VerticalAngleUnits = message[6][0];
			SlopeDistance = double.Parse(message[7], CultureInfo.InvariantCulture);
			SlopeDistanceUnits = message[8][0];
		}

		public string HorizontalVector { get; private set; }

		public double HorizontalDistance { get; private set; }

		public char HorizontalDistanceUnits { get; private set; }

		public double HorizontalAngle { get; private set; }

		public char HorizontalAngleUnits { get; private set; }

		public double VerticalAngle { get; private set; }

		public char VerticalAngleUnits { get; private set; }

		public double SlopeDistance { get; private set; }

		public char SlopeDistanceUnits { get; private set; }
	}
}
