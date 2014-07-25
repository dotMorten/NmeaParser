using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
#if NETFX_CORE
using BTDevice = Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService;
using Windows.Devices.Bluetooth.Rfcomm;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
#else
using BTDevice = Windows.Networking.Proximity.PeerInformation;
#endif

namespace NmeaParser
{
	public class BluetoothDevice : NmeaDevice
	{
		private BTDevice m_device;
		private StreamSocket m_socket;

		public BluetoothDevice(BTDevice device)
		{
			m_device = device;
		}

		protected override async Task<System.IO.Stream> OpenStreamAsync()
		{
			var socket = new Windows.Networking.Sockets.StreamSocket();
			await socket.ConnectAsync(
#if NETFX_CORE
				m_device.ConnectionHostName,
								m_device.ConnectionServiceName);
#else
								m_device.HostName, "1");
#endif
			m_socket = socket;
			return socket.InputStream.AsStreamForRead();
		}

		protected override Task CloseStreamAsync(System.IO.Stream stream)
		{
			stream.Dispose();
			m_socket.Dispose();
			m_socket = null;
			return Task.FromResult(true);
		}
	}
}
