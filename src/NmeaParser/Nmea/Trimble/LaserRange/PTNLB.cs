using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTDevices.Nmea.Trimble.LaserRange
{
	/// <summary>
	/// Tree Measurement
	/// </summary>
	[NmeaMessageType(Type = "PTNLB")]
	public class Ptnlb : NmeaMessage
	{
		protected override void LoadMessage(string[] message)
		{
			TreeHeight = message[0];
			MeasuredTreeHeight = double.Parse(message[1], CultureInfo.InvariantCulture);
			MeasuredTreeHeightUnits = message[2][0];
			TreeDiameter = message[3];
			MeasuredTreeDiameter = double.Parse(message[4], CultureInfo.InvariantCulture);
			MeasuredTreeDiameterUnits = message[5][0];
		}

		public string TreeHeight { get; private set; }

		public double MeasuredTreeHeight { get; private set; }

		public char MeasuredTreeHeightUnits { get; private set; }

		public string TreeDiameter { get; private set; }

		public double MeasuredTreeDiameter { get; private set; }

		public char MeasuredTreeDiameterUnits { get; private set; }

		//more to do...
	
	}
}
