using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser.Nmea.LaserTech.LaserRange
{
	/// <summary>
	/// Laser Range 
	/// </summary>
	[NmeaMessageType(Type = "PLTIT")]
	public class Pltit : LaserRangeMessage
	{
	}
}
