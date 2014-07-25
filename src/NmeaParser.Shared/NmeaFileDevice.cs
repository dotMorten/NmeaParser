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
	public class NmeaFileDevice : NmeaDevice
	{
#if NETFX_CORE
		Windows.Storage.IStorageFile m_filename;
#else
		string m_filename;
#endif
		BufferedStream m_stream;
		int m_readSpeed;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="readSpeed">The time to wait between each line being read in milliseconds</param>
#if NETFX_CORE
		public NmeaFileDevice(Windows.Storage.IStorageFile filename, int readSpeed = 200)
#else
		public NmeaFileDevice(string filename, int readSpeed = 200)
#endif
		{
			m_filename = filename;
			m_readSpeed = readSpeed;
		}

#if NETFX_CORE
		protected async override Task<System.IO.Stream> OpenStreamAsync()
		{
			var stream = await m_filename.OpenStreamForReadAsync();
			StreamReader sr = new StreamReader(stream);
			m_stream = new BufferedStream(sr, m_readSpeed);
			return m_stream;
#else
		protected override Task<System.IO.Stream> OpenStreamAsync()
		{
			StreamReader sr = System.IO.File.OpenText(m_filename);
			m_stream = new BufferedStream(sr, m_readSpeed);
			return Task.FromResult<Stream>(m_stream);
#endif
		}

		protected override Task CloseStreamAsync(System.IO.Stream stream)
		{
			m_stream.Dispose();
			return Task.FromResult(true);
		}

		// stream that slowly populates a buffer from a StreamReader to simulate nmea messages coming in line by line
		// at a steady stream
		private class BufferedStream : Stream
		{
			StreamReader m_sr;
			byte[] buffer = new byte[0];
			System.Threading.Timer timer;
			object lockObj = new object();
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
			public override bool CanRead { get { return true; } }
			public override bool CanSeek { get { return false; } }
			public override bool CanWrite { get { return false; } }
			public override void Flush() { }

			public override long Length { get { return m_sr.BaseStream.Length; } }

			public override long Position
			{
				get { return m_sr.BaseStream.Position; }
				set
				{
					throw new NotSupportedException();
				}
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				lock (lockObj)
				{
					if(this.buffer.Length <= count)
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

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotSupportedException();
			}

			public override void SetLength(long value)
			{
				throw new NotSupportedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				throw new NotSupportedException();
			}
			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				m_sr.Dispose();
				timer.Dispose();
			}
		}
	}
}
