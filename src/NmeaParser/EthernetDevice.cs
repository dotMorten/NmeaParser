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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NmeaParser
{
	/// <summary>
	/// Ethernet device
	/// </summary>
    public class EthernetDevice : NmeaDevice
    {
		private Socket m_socket;

		/// <summary>
		/// Initializes a new instance of the <see cref="EthernetDevice"/> class.
		/// </summary>
		/// <param name="socket">Socket (use TcpClient.Client for TCP or UdpClient.Client for UDP)</param>
		public EthernetDevice(Socket socket) : base()
		{
			m_socket = socket;
		}

        /// <inheritdoc />
        protected override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (m_socket == null)
                throw new InvalidOperationException("Device not open");
            return Task.FromResult(m_socket.Receive(buffer, offset, count, SocketFlags.None));
        }

        /// <summary>
        /// Closes the stream asynchronous.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        protected override Task CloseStreamAsync(System.IO.Stream stream)
		{
			return Task.FromResult(true); //do nothing
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (m_socket != null)
				m_socket.Dispose();
			m_socket = null;
		}

        /// <inheritdoc />
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override Task WriteAsync(byte[] buffer, int offset, int length)
        {
            if (m_socket == null)
                throw new InvalidOperationException("Device not open");
            return Task.FromResult(m_socket.Send(buffer, offset, length, SocketFlags.None));
        }

        /// <inheritdoc />
        protected override Task<Stream> OpenStreamAsync()
        {
            return Task.FromResult<Stream>(null);
        }
    }
}
