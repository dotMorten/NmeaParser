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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser
{
	/// <summary>
	/// An abstract generic NMEA device that reads a stream at a decreased pace,
	/// mostly used to emulate NMEA input from files and strings.
	/// </summary>
	public abstract class BufferedStreamDevice : NmeaDevice
	{
		BufferedStream m_stream;
		int m_readSpeed;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="readSpeed">The time to wait between each line being read in milliseconds</param>
		protected BufferedStreamDevice( int readSpeed = 200)
		{
			m_readSpeed = readSpeed;
		}

		/// <summary>
		/// Gets the stream to perform buffer reads on.
		/// </summary>
		/// <returns></returns>
		protected abstract Task<System.IO.Stream> GetStreamAsync();

		/// <summary>
		/// Opens the stream asynchronous.
		/// </summary>
		/// <returns></returns>
		protected sealed async override Task<System.IO.Stream> OpenStreamAsync()
		{
			var stream = await GetStreamAsync();
			StreamReader sr = new StreamReader(stream);
			m_stream = new BufferedStream(sr, m_readSpeed);
			return m_stream;
		}

		/// <summary>
		/// Closes the stream asynchronous.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <returns></returns>
		protected override Task CloseStreamAsync(System.IO.Stream stream)
		{
			m_stream.Dispose();
			return Task.FromResult(true);
		}

		// stream that slowly populates a buffer from a StreamReader to simulate nmea messages coming
		// in line by line at a steady stream
		private class BufferedStream : Stream
		{
			StreamReader m_sr;
			byte[] buffer = new byte[0];
			System.Threading.Timer timer;
			object lockObj = new object();
			/// <summary>
			/// Initializes a new instance of the <see cref="BufferedStream"/> class.
			/// </summary>
			/// <param name="stream">The stream.</param>
			/// <param name="readSpeed">The read speed.</param>
			public BufferedStream(StreamReader stream, int readSpeed)
			{
				m_sr = stream;
				timer = new System.Threading.Timer(OnRead, null, 0, readSpeed); //add a new line to buffer every 100 ms
			}
			private void OnRead(object state)
			{
				if (m_sr.EndOfStream)
					m_sr.BaseStream.Seek(0, SeekOrigin.Begin); //start over

				//populate the buffer with a line
				string line = m_sr.ReadLine() + '\n';
				var bytes = Encoding.UTF8.GetBytes(line);
				lock (lockObj)
				{
					byte[] newBuffer = new byte[buffer.Length + bytes.Length];
					buffer.CopyTo(newBuffer, 0);
					bytes.CopyTo(newBuffer, buffer.Length);
					buffer = newBuffer;
				}
			}
			/// <summary>
			/// Gets a value indicating whether this instance can read.
			/// </summary>
			/// <value>
			///   <c>true</c> if this instance can read; otherwise, <c>false</c>.
			/// </value>
			public override bool CanRead { get { return true; } }
			/// <summary>
			/// Gets a value indicating whether this instance can seek.
			/// </summary>
			/// <value>
			///   <c>true</c> if this instance can seek; otherwise, <c>false</c>.
			/// </value>
			public override bool CanSeek { get { return false; } }
			/// <summary>
			/// Gets a value indicating whether this instance can write.
			/// </summary>
			/// <value>
			///   <c>true</c> if this instance can write; otherwise, <c>false</c>.
			/// </value>
			public override bool CanWrite { get { return false; } }
			/// <summary>
			/// Flushes this instance.
			/// </summary>
			public override void Flush() { }
			/// <summary>
			/// Gets the length.
			/// </summary>
			/// <value>
			/// The length.
			/// </value>
			public override long Length { get { return m_sr.BaseStream.Length; } }

			/// <summary>
			/// Gets or sets the position.
			/// </summary>
			/// <value>
			/// The position.
			/// </value>
			/// <exception cref="System.NotSupportedException"></exception>
			public override long Position
			{
				get { return m_sr.BaseStream.Position; }
				set
				{
					throw new NotSupportedException();
				}
			}
			/// <summary>
			/// Reads the specified buffer.
			/// </summary>
			/// <param name="buffer">The buffer.</param>
			/// <param name="offset">The offset.</param>
			/// <param name="count">The count.</param>
			/// <returns></returns>
			public override int Read(byte[] buffer, int offset, int count)
			{
				lock (lockObj)
				{
					if (this.buffer.Length <= count)
					{
						int length = this.buffer.Length;
						this.buffer.CopyTo(buffer, 0);
						this.buffer = new byte[0];
						return length;
					}
					else
					{
						Array.Copy(this.buffer, buffer, count);
						byte[] newBuffer = new byte[this.buffer.Length - count];
						Array.Copy(this.buffer, count, newBuffer, 0, newBuffer.Length);
						this.buffer = newBuffer;
						return count;
					}
				}
			}

			/// <summary>
			/// Seeks the specified offset.
			/// </summary>
			/// <param name="offset">The offset.</param>
			/// <param name="origin">The origin.</param>
			/// <returns></returns>
			/// <exception cref="System.NotSupportedException"></exception>
			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotSupportedException();
			}

			/// <summary>
			/// Sets the length.
			/// </summary>
			/// <param name="value">The value.</param>
			/// <exception cref="System.NotSupportedException"></exception>
			public override void SetLength(long value)
			{
				throw new NotSupportedException();
			}

			/// <summary>
			/// Writes the specified buffer.
			/// </summary>
			/// <param name="buffer">The buffer.</param>
			/// <param name="offset">The offset.</param>
			/// <param name="count">The count.</param>
			/// <exception cref="System.NotSupportedException"></exception>
			public override void Write(byte[] buffer, int offset, int count)
			{
				throw new NotSupportedException();
			}

			/// <summary>
			/// Releases unmanaged and - optionally - managed resources.
			/// </summary>
			/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				m_sr.Dispose();
				timer.Dispose();
			}
		}
	}
}
