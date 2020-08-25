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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
        private readonly BurstEmulationSettings emulationSettings = new BurstEmulationSettings();

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedStreamDevice"/> class.
        /// </summary>
        protected BufferedStreamDevice() : this(1000)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedStreamDevice"/> class.
        /// </summary>
        /// <param name="burstRate">The time to wait between each group of lines being read in milliseconds</param>
        protected BufferedStreamDevice(int burstRate)
        {
            BurstRate = TimeSpan.FromMilliseconds(burstRate);
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
            var stream = await GetStreamAsync().ConfigureAwait(false);
            StreamReader sr = new StreamReader(stream);
            m_stream = new BufferedStream(sr, emulationSettings);
            m_stream.EndOfStreamReached += OnEndOfStreamReached;
            return m_stream;
        }

        private void OnEndOfStreamReached(object sender, EventArgs e)
        {
            EndOfStreamReached?.Invoke(this, e);
            if (m_stream is BufferedStream stream && !stream.CanRewind && IsOpen)
            {
                // If we can't rewind the stream, stop
                _ = CloseAsync();
            }
        }

        /// <summary>
        /// Gets or sets the emulated baud rate. Defaults to 115200
        /// </summary>
        /// <remarks>
        /// Note that if the baud rate gets very low, while keeping a high <see cref="BurstRate"/>, the stream will not be able to keep
        /// up the burstrate. For high-frequency bursts, make sure you have a corresponding high emualated baud rate.
        /// </remarks>
        public uint EmulatedBaudRate
        {
            get => emulationSettings.EmulatedBaudRate;
            set => emulationSettings.EmulatedBaudRate = value;
        }

        /// <summary>
        /// Gets or sets the emulated burst rate - that is the frequency of each burst of messages. Defaults to 1 second (1hz).
        /// </summary>
        /// <remarks>
        /// Note that if the burst rate gets very high, while keeping a low <see cref="EmulatedBaudRate"/>, the stream will not be able to keep
        /// up the burstrate. For high-frequency bursts, make sure you have a corresponding high emualated baud rate.
        /// </remarks>
        public TimeSpan BurstRate
        {
            get => emulationSettings.BurstRate;
            set
            {
                if (value.TotalMilliseconds < 1)
                    throw new ArgumentOutOfRangeException(nameof(BurstRate), "Burst rate must be at least 1 ms");
                emulationSettings.BurstRate = value;
            }
        }

        /// <summary>
        /// Gets or sets the separator between each burst of data. Defaults to <see cref="BurstEmulationSeparator.FirstToken"/>.
        /// </summary>
        public BurstEmulationSeparator BurstSeparator
        {
            get => emulationSettings.Separator;
            set => emulationSettings.Separator = value;
        }

        /// <inheritdoc />
        protected override Task CloseStreamAsync(System.IO.Stream stream)
        {
            if (m_stream != null)
            {
                m_stream.EndOfStreamReached -= OnEndOfStreamReached;
                m_stream?.Dispose();
            }
            return Task.FromResult(true);
        }

        private class BurstEmulationSettings
        {
            public uint EmulatedBaudRate { get; set; } = 115200;
            public TimeSpan BurstRate { get; set; } = TimeSpan.FromSeconds(1);
            public BurstEmulationSeparator Separator { get; set; }
        }

        /// <summary>
        /// Defined how a burst of data is separated
        /// </summary>
        /// <seealso cref="BufferedStreamDevice.BurstSeparator"/>
        public enum BurstEmulationSeparator
        {
            /// <summary>
            /// The first NMEA token encountered will be used as an indicator for pauses between bursts
            /// </summary>
            FirstToken,
            /// <summary>
            /// An empty line in the NMEA stream should indicate a pause in the burst of messages
            /// </summary>
            EmptyLine
        }

        /// <summary>
        /// Raised when the stream has reached the end. If the stream can be revound, it'll start over, unless you stop the device in this thread.
        /// </summary>
        public event EventHandler? EndOfStreamReached;

        // stream that slowly populates a buffer from a StreamReader to simulate nmea messages coming
        // in lastLineRead by lastLineRead at a steady stream
        private class BufferedStream : Stream
        {
            private readonly StreamReader m_sr;
            private byte[] m_buffer = new byte[0];
            private readonly object lockObj = new object();
            private string? groupToken = null;
            private BurstEmulationSettings m_settings;
            private CancellationTokenSource m_tcs;
            private Task m_readTask;
            
            /// <summary>
            /// Initializes a new instance of the <see cref="BufferedStream"/> class.
            /// </summary>
            /// <param name="stream">The stream.</param>
            /// <param name="settings">Emulation settings.</param>
            public BufferedStream(StreamReader stream, BurstEmulationSettings settings)
            {
                m_settings = settings;
                m_sr = stream;
                m_tcs = new CancellationTokenSource();
                m_readTask = StartReadLoop(m_tcs.Token);
            }

            internal bool CanRewind => m_sr.BaseStream.CanSeek;

            private async Task StartReadLoop(CancellationToken cancellationToken)
            {
                await Task.Yield();
                var start = Stopwatch.GetTimestamp();
                while (!cancellationToken.IsCancellationRequested)
                {
                    var line = ReadLine();
                    if (line != null)
                    {
                        // Group token is the first message type received - every time we see it, we'll take a short burst break
                        if (groupToken == null && line.StartsWith("$", StringComparison.Ordinal))
                        {
                            var values = line.Trim().Split(new char[] { ',' });
                            if (values.Length > 0)
                                groupToken = values[0];
                        }
                        if (m_settings.Separator == BurstEmulationSeparator.EmptyLine && string.IsNullOrWhiteSpace(line) ||
                            m_settings.Separator == BurstEmulationSeparator.FirstToken && groupToken != null && line.StartsWith(groupToken, StringComparison.Ordinal))
                        {
                            // Emulate the burst pause
                            var now = Stopwatch.GetTimestamp();
                            var delay = (now - start) / (double)Stopwatch.Frequency;
                            if (delay < m_settings.BurstRate.TotalSeconds)
                                await Task.Delay(TimeSpan.FromSeconds(m_settings.BurstRate.TotalSeconds - delay)).ConfigureAwait(false);
                            else
                            {
                                Debug.WriteLine("Warning: baud rate too slow for amount of data, or burst rate too fast");
                            }
                            if (cancellationToken.IsCancellationRequested)
                                return;
                            start = Stopwatch.GetTimestamp();
                        }
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            await AppendToBuffer(line).ConfigureAwait(false);
                        }
                    }
                }
            }

            private double pendingDelay = 0;

            private async Task AppendToBuffer(string line)
            {
                var bytes = Encoding.UTF8.GetBytes(line);
                lock (lockObj)
                {
                    byte[] newBuffer = new byte[m_buffer.Length + bytes.Length];
                    m_buffer.CopyTo(newBuffer, 0);
                    bytes.CopyTo(newBuffer, m_buffer.Length);
                    m_buffer = newBuffer;
                }
                var delay = bytes.Length * 10d / m_settings.EmulatedBaudRate; // 8 bits + 1 parity + 1 stop bit = 10bits per byte;

                pendingDelay += delay;
                if (pendingDelay < 0.016) //No reason to wait under the 16ms - Task.Delay not that accurate anyway
                {
                    return;
                }
                // Task.Delay isn't very accurate so use the stopwatch to get the real delay and use difference to fix it later
                var start = Stopwatch.GetTimestamp();
                await Task.Delay(TimeSpan.FromSeconds(pendingDelay)).ConfigureAwait(false);
                var end = Stopwatch.GetTimestamp();
                pendingDelay -= (end - start) / (double)Stopwatch.Frequency;
            }

            private string? ReadLine()
            {
                if (m_tcs.IsCancellationRequested)
                    return null;
                if (m_sr.EndOfStream)
                {
                    EndOfStreamReached?.Invoke(this, EventArgs.Empty);
                    if (m_tcs.IsCancellationRequested)
                        return null;
                    m_sr.BaseStream.Seek(0, SeekOrigin.Begin); //start over
                }
                return m_sr.ReadLine() + '\n';
            }

            /// <inheritdoc />
            public override bool CanRead => true;

            /// <inheritdoc />
            public override bool CanSeek => false;

            /// <inheritdoc />
            public override bool CanWrite => false;

            /// <inheritdoc />
            public override void Flush() { }

            /// <inheritdoc />
            public override long Length => m_sr.BaseStream.Length;

            /// <inheritdoc />
            public override long Position
            {
                get => m_sr.BaseStream.Position;
                set => throw new NotSupportedException();
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
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            /// <inheritdoc />
            public override void SetLength(long value) => throw new NotSupportedException();

            /// <inheritdoc />
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            /// <inheritdoc />
            protected override void Dispose(bool disposing)
            {
                m_tcs.Cancel();
                m_sr.Dispose();
                base.Dispose(disposing);
            }

            internal event EventHandler? EndOfStreamReached;
        }
    }
}
