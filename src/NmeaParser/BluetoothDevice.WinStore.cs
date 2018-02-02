//
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
#if NETFX_CORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Devices.Bluetooth.Rfcomm;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
#if WINDOWS_UWP
using Windows.Networking.Proximity;
#endif

namespace NmeaParser
{
	/// <summary>
	/// A Bluetooth NMEA device
	/// </summary>
	public class BluetoothDevice : NmeaDevice
	{
		private Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService m_deviceService;
#if WINDOWS_UWP
        private Windows.Networking.Proximity.PeerInformation m_devicePeer;
#endif
        private StreamSocket m_socket;

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothDevice"/> class.
		/// </summary>
		/// <param name="service">The RF Comm Device service.</param>
		public BluetoothDevice(Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService service)
		{
			m_deviceService = service;
		}

#if WINDOWS_UWP

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothDevice"/> class.
        /// </summary>
        /// <param name="peer">The peer information device.</param>
        public BluetoothDevice(Windows.Networking.Proximity.PeerInformation peer)
        {
            m_devicePeer = peer;
        }
#endif

        /// <summary>
        /// Creates the stream the NmeaDevice is working on top off.
        /// </summary>
        /// <returns></returns>
        protected override async Task<System.IO.Stream> OpenStreamAsync()
		{
			var socket = new Windows.Networking.Sockets.StreamSocket();
#if WINDOWS_UWP
            if (m_devicePeer != null)
            {
                await socket.ConnectAsync(m_devicePeer.HostName, "1");
            }
            else
#endif
            {
                await socket.ConnectAsync(m_deviceService.ConnectionHostName, m_deviceService.ConnectionServiceName);
            }
			m_socket = socket;
			return socket.InputStream.AsStreamForRead();
		}

		/// <summary>
		/// Closes the stream the NmeaDevice is working on top off.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <returns></returns>
		protected override Task CloseStreamAsync(System.IO.Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");
			stream.Dispose();
			m_socket.Dispose();
			m_socket = null;
			return Task.FromResult(true);
		}
	}
}
#endif