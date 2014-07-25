using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NETFX_CORE
using BTDevice = Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService;
using Windows.Devices.Bluetooth.Rfcomm;
#else
using BTDevice = Windows.Networking.Proximity.PeerInformation;
#endif
using Windows.Foundation;

namespace NmeaParser
{
	public class NmeaDevice : Device
	{
		private string message = "";
		public NmeaDevice(BTDevice device)
			: base(device)
		{
		}
		protected override void OnData(byte[] data)
		{
			var nmea = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
			message += nmea;
			var lineEnd = message.IndexOf("\n");
			if (lineEnd > -1)
			{
				string line = message.Substring(0, lineEnd);
				message = message.Substring(lineEnd).Trim();
				ProcessMessage(line.Trim());
			}
		}

		private void ProcessMessage(string p)
		{
			try
			{
				var msg = NmeaParser.Nmea.NmeaMessage.Parse(p);
				if (msg != null)
					OnMessageReceived(msg);
			}
			catch { }
		}

		private void OnMessageReceived(Nmea.NmeaMessage msg)
		{
			if (MessageReceived != null)
				MessageReceived(this, msg);
		}

		public event TypedEventHandler<NmeaDevice, Nmea.NmeaMessage> MessageReceived;
	}
}
