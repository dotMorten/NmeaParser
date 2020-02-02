//  *******************************************************************************
//  *  Licensed under the Apache License, Version 2.0 (the "License");
//  *  you may not use this file except in compliance with the License.
//  *  You may obtain a copy of the License at
//  *
//  *  http://www.apache.org/licenses/LICENSE-2.0
//  *
//  *   Unless required by applicable law or agreed to in writing, software
//  *   distributed under the License is distributed on an "AS IS" BASIS,
//  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *   See the License for the specific language governing permissions and
//  *   limitations under the License.
//  ******************************************************************************

#if NETFX || NETCOREAPP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace NmeaParser
{
    /// <summary>
    /// A Serial Port NMEA device
    /// </summary>
    public class SerialPortDevice : NmeaDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerialPortDevice" /> class.
        /// </summary>
        /// <param name="port">The serial port.</param>
        /// <exception cref="System.ArgumentNullException">port</exception>
        public SerialPortDevice(System.IO.Ports.SerialPort port)
        {
            if (port == null)
                throw new ArgumentNullException("port");
            Port = port;
        }

        /// <summary>
        /// Gets the active serial port.
        /// </summary>
        public System.IO.Ports.SerialPort Port { get; }

        /// <inheritdoc />
        protected override Task<System.IO.Stream> OpenStreamAsync()
        {
            if (!Port.IsOpen)
                Port.Open();
            return Task.FromResult<System.IO.Stream>(Port.BaseStream);
        }

        /// <inheritdoc />
        protected override Task CloseStreamAsync(System.IO.Stream stream)
        {
            if (Port.IsOpen)
                Port.Close();
            return Task.FromResult<object?>(null);
        }

        /// <summary>
        /// Writes data to the serial port (useful for RTCM/dGPS scenarios)
        /// </summary>
        /// <param name="buffer">The byte array that contains the data to write to the port.</param>
        /// <param name="offset">The zero-based byte offset in the buffer parameter at which to begin copying 
        /// bytes to the port.</param>
        /// <param name="count">The number of bytes to write.</param>
        [Obsolete("Use WriteAsync")]
        public void Write(byte[] buffer, int offset, int count)
        {
            Port.Write(buffer, offset, count);
        }

        /// <inheritdoc />
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override Task WriteAsync(byte[] buffer, int offset, int length)
        {
            if (!Port.IsOpen)
                throw new InvalidOperationException("Device not open");

            Port.Write(buffer, offset, length);
            return Task.FromResult<object?>(null);
        }
    }
}
#endif