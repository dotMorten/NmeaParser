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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;

namespace NmeaParser
{
	/// <summary>
	/// A Serial Port NMEA device
	/// </summary>
	public class SerialPortDevice : NmeaDevice
	{
		private SerialDevice m_port;

		/// <summary>
		/// Initializes a new instance of the <see cref="SerialPortDevice" /> class.
		/// </summary>
		/// <param name="device">The serial port.</param>
		/// <exception cref="System.ArgumentNullException">port</exception>
		public SerialPortDevice(SerialDevice device)
		{
			if (device == null)
				throw new ArgumentNullException("device");
			m_port = device;
		}

		/// <summary>
		/// Gets the active serial port.
		/// </summary>
		public SerialDevice SerialDevice
		{
			get
			{
				return m_port;
			}
		}

		/// <summary>
		/// Creates the stream the NmeaDevice is working on top off.
		/// </summary>
		/// <returns></returns>
		protected override Task<System.IO.Stream> OpenStreamAsync()
		{
			return Task.FromResult<System.IO.Stream>(m_port.InputStream.AsStreamForRead(0));
		}

		/// <summary>
		/// Closes the stream the NmeaDevice is working on top off.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <returns></returns>
		protected override Task CloseStreamAsync(System.IO.Stream stream)
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Writes data to the serial port (useful for RTCM/dGPS scenarios)
		/// </summary>
		/// <param name="buffer">The byte array that contains the data to write to the port.</param>
		/// <param name="offset">The zero-based byte offset in the buffer parameter at which to begin copying 
		/// bytes to the port.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Write(byte[] buffer, int offset, int count)
		{
			m_port.OutputStream.AsStreamForWrite().Write(buffer, offset, count);
		}
	}
}
