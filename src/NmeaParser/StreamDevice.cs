﻿//
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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser
{
	/// <summary>
	/// Generic stream device
	/// </summary>
    public class StreamDevice : NmeaDevice
    {
		System.IO.Stream m_stream;
		/// <summary>
		/// Initializes a new instance of the <see cref="StreamDevice"/> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
		public StreamDevice(Stream stream) : base()
		{
			m_stream = stream;
		}

		/// <summary>
		/// Opens the stream asynchronous.
		/// </summary>
		/// <returns></returns>
		protected override Task<Stream> OpenStreamAsync()
		{
			return Task.FromResult(m_stream);
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
			if (m_stream != null)
				m_stream.Dispose();
			m_stream = null;
		}


        /// <inheritdoc />
        public override bool CanWrite => m_stream?.CanWrite == true;

        /// <inheritdoc />
        public override Task WriteAsync(byte[] buffer, int offset, int length)
        {
            if (m_stream == null)
                throw new InvalidOperationException("Device not open");
            return m_stream.WriteAsync(buffer, offset, length);
        }
    }
}
