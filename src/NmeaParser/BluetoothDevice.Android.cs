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
#if __ANDROID__
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Android.Bluetooth;

namespace NmeaParser
{
	/// <summary>
	/// A Bluetooth NMEA device
	/// </summary>
	public class BluetoothDevice : NmeaDevice
	{
        private static Java.Util.UUID SERIAL_UUID = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private Android.Bluetooth.BluetoothDevice m_device;
        private BluetoothSocket m_socket;

        /// <summary>
        /// Gets a list of bluetooth devices that supports serial communication
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Android.Bluetooth.BluetoothDevice> GetBluetoothSerialDevices()
        {
            var adapter = Android.Bluetooth.BluetoothAdapter.DefaultAdapter;
            if (adapter != null && adapter.IsEnabled)
            {
                foreach (var b in adapter.BondedDevices.Where(d => d.GetUuids().Where(t => t.Uuid.ToString().Equals("00001101-0000-1000-8000-00805F9B34FB", StringComparison.InvariantCultureIgnoreCase)).Any()))
                    yield return b;
            }
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothDevice"/> class.
		/// </summary>
		/// <param name="device">The Android Bluetooth Device.</param>
		public BluetoothDevice(Android.Bluetooth.BluetoothDevice device)
		{
            m_device = device;
		}
        
        /// <summary>
        /// Creates the stream the NmeaDevice is working on top off.
        /// </summary>
        /// <returns></returns>
        protected override Task<System.IO.Stream> OpenStreamAsync()
        {
            var adapter = Android.Bluetooth.BluetoothAdapter.DefaultAdapter;
            if (adapter?.IsEnabled != true)
                throw new InvalidOperationException("Bluetooth Adapter not enabled");
            var d = adapter.GetRemoteDevice(m_device.Address);
            var socket = d.CreateRfcommSocketToServiceRecord(SERIAL_UUID);
            socket.Connect();
			m_socket = socket;
            return Task.FromResult<Stream>(socket.InputStream);
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

        /// <inheritdoc />
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override Task WriteAsync(byte[] buffer, int offset, int length)
        {
            if (m_socket == null)
                throw new InvalidOperationException("Device not open");
            return m_socket.OutputStream.WriteAsync(buffer, offset, length);
        }
	}
}
#endif