using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTDevices.Nmea
{
	public class UnknownMessage : NmeaMessage
	{
		public string[] Values { get { return base.MessageParts; } }
		protected override void LoadMessage(string[] message)
		{
		}
	}
}
