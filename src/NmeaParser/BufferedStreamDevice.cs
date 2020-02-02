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
        private BufferedStream? m_stream;
        private readonly int m_readSpeed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedStreamDevice"/> class.
        /// </summary>
        protected BufferedStreamDevice() : this(1000)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedStreamDevice"/> class.
        /// </summary>
        /// <param name="readSpeed">The time to wait between each group of lines being read in milliseconds</param>
        protected BufferedStreamDevice(int readSpeed)
        {
            m_readSpeed = readSpeed;
        }

        /// <summary>
        /// Gets the stream to perform buffer reads on.
        /// </summary>
        /// <returns>The opened data stream.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected abstract Task<System.IO.Stream> GetStreamAsync();

        /// <inheritdoc />
        protected sealed async override Task<System.IO.Stream> OpenStreamAsync()
        {
            var stream = await GetStreamAsync();
            StreamReader sr = new StreamReader(stream);
            m_stream = new BufferedStream(sr, m_readSpeed);
            return m_stream;
        }

        /// <inheritdoc />
        protected override Task CloseStreamAsync(System.IO.Stream stream)
        {
            m_stream?.Dispose();
            return Task.FromResult(true);
        }

        // stream that slowly populates a buffer from a StreamReader to simulate nmea messages coming
        // in lastLineRead by lastLineRead at a steady stream
        private class BufferedStream : Stream
        {
            private readonly StreamReader m_sr;
            private byte[] m_buffer = new byte[0];
            private readonly System.Threading.Timer m_timer;
            private readonly object lockObj = new object();
            private string? groupToken = null;
            private string? lastLineRead = null;
            /// <summary>
            /// Initializes a new instance of the <see cref="BufferedStream"/> class.
            /// </summary>
            /// <param name="stream">The stream.</param>
            /// <param name="readSpeed">The read speed in milliseconds.</param>
            public BufferedStream(StreamReader stream, int readSpeed)
            {
                m_sr = stream;
                m_timer = new System.Threading.Timer(OnRead, null, 0, readSpeed); //read a group of lines every 'readSpeed' milliseconds
            }
            private void OnRead(object state)
            {
                if (lastLineRead != null)
                    AppendToBuffer(lastLineRead);
                //Get the group token if we don't have one
                while (groupToken == null && (lastLineRead == null || !lastLineRead.StartsWith("$", StringComparison.Ordinal)))
                {
                    lastLineRead = ReadLine(); //seek forward to first nmea token
                    AppendToBuffer(lastLineRead);
                }
                if(groupToken == null && lastLineRead != null)
                {
                    var values = lastLineRead.Trim().Split(new char[] { ',' });
                    if (values.Length > 0)
                        groupToken = values[0];
                }
                lastLineRead = ReadLine();
                while (!lastLineRead.StartsWith(groupToken, StringComparison.Ordinal)) //keep reading until messages start repeating again
                {
                    AppendToBuffer(lastLineRead);
                    lastLineRead = ReadLine();
                }                
            }
            private void AppendToBuffer(string line)
            {
                var bytes = Encoding.UTF8.GetBytes(line);
                lock (lockObj)
                {
                    byte[] newBuffer = new byte[m_buffer.Length + bytes.Length];
                    m_buffer.CopyTo(newBuffer, 0);
                    bytes.CopyTo(newBuffer, m_buffer.Length);
                    m_buffer = newBuffer;
                }
            }
            private string ReadLine()
            {
                if (m_sr.EndOfStream)
                    m_sr.BaseStream.Seek(0, SeekOrigin.Begin); //start over
                return m_sr.ReadLine() + '\n';
            }
            
            /// <inheritdoc />
            public override bool CanRead { get { return true; } }

            /// <inheritdoc />
            public override bool CanSeek { get { return false; } }

            /// <inheritdoc />
            public override bool CanWrite { get { return false; } }

            /// <inheritdoc />
            public override void Flush() { }

            /// <inheritdoc />
            public override long Length { get { return m_sr.BaseStream.Length; } }

            /// <inheritdoc />
            public override long Position
            {
                get { return m_sr.BaseStream.Position; }
                set
                {
                    throw new NotSupportedException();
                }
            }

            /// <inheritdoc />
            public override int Read(byte[] buffer, int offset, int count)
            {
                lock (lockObj)
                {
                    if (this.m_buffer.Length <= count)
                    {
                        int length = this.m_buffer.Length;
                        this.m_buffer.CopyTo(buffer, 0);
                        this.m_buffer = new byte[0];
                        return length;
                    }
                    else
                    {
                        Array.Copy(this.m_buffer, buffer, count);
                        byte[] newBuffer = new byte[this.m_buffer.Length - count];
                        Array.Copy(this.m_buffer, count, newBuffer, 0, newBuffer.Length);
                        this.m_buffer = newBuffer;
                        return count;
                    }
                }
            }

            /// <inheritdoc />
            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc />
            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc />
            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc />
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                m_sr.Dispose();
                m_timer.Dispose();
            }
        }
    }
}
