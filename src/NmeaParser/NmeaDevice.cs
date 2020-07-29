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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using NmeaParser.Messages;

namespace NmeaParser
{
    /// <summary>
    /// A generic abstract NMEA device
    /// </summary>
    public abstract class NmeaDevice : IDisposable
    {
        private readonly object m_lockObject = new object();
        private string m_message = "";
        private Stream? m_stream;
        private CancellationTokenSource? m_cts;
        private bool m_isOpening;
        private Task? m_ParserTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="NmeaDevice"/> class.
        /// </summary>
        protected NmeaDevice()
        {
        }

        /// <summary>
        /// Creates and opens the stream the <see cref="NmeaDevice"/> will be working on top off.
        /// </summary>
        /// <returns>A task that represents the asynchronous action.</returns>
        public async Task OpenAsync()
        {
            lock (m_lockObject)
            {
                if (IsOpen || m_isOpening) return;
                m_isOpening = true;
            }
            m_cts = new CancellationTokenSource();
            m_stream = await OpenStreamAsync();
            StartParser(m_cts.Token);
            _lastMultiMessage = null;
            lock (m_lockObject)
            {
                IsOpen = true;
                m_isOpening = false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "_")]
        private void StartParser(CancellationToken token)
        {
            System.Diagnostics.Debug.WriteLine("Starting parser...");
            m_ParserTask = Task.Run(async () =>
            {
                byte[] buffer = new byte[1024];
                while (!token.IsCancellationRequested)
                {
                    int readCount = 0;
                    try
                    {
                        readCount = await ReadAsync(buffer, 0, 1024, token).ConfigureAwait(false);
                    }
                    catch { }
                    if (token.IsCancellationRequested)
                        break;
                    if (readCount > 0)
                    {
                        OnData(buffer, readCount);
                    }
                    await Task.Yield();
                }
            });
        }

        /// <summary>
        /// Performs a read operation of the stream
        /// </summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The value of the TResult
        /// parameter contains the total number of bytes read into the buffer. The result
        /// value can be less than the number of bytes requested if the number of bytes currently
        /// available is less than the requested number, or it can be 0 (zero) if the end
        /// of the stream has been reached.
        /// </returns>
        protected virtual Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (m_stream == null)
                return Task.FromResult(0);
            return m_stream.ReadAsync(buffer, 0, 1024, cancellationToken);
        }

        /// <summary>
        /// Creates and opens the stream the NmeaDevice is working on top off.
        /// </summary>
        /// <returns>The opened data stream.</returns>
        /// <seealso cref="CloseStreamAsync(Stream)"/>
        protected abstract Task<Stream> OpenStreamAsync();

        /// <summary>
        /// Closes the device.
        /// </summary>
        /// <returns>A task that represents the asynchronous action.</returns>
        public async Task CloseAsync()
        {
            if (m_cts != null)
            {
                if (m_cts != null)
                    m_cts.Cancel();
                m_cts = null;
            }
            if (m_ParserTask != null)
                await m_ParserTask;
            if (m_stream != null)
                await CloseStreamAsync(m_stream);
            _lastMultiMessage = null;
            m_stream = null;
            lock (m_lockObject)
            {
                m_isOpening = false;
                IsOpen = false;
            }
        }
        /// <summary>
        /// Closes the stream the NmeaDevice is working on top off.
        /// </summary>
        /// <param name="stream">The stream to be closed.</param>
        /// <returns>A task that represents the asynchronous action.</returns>
        /// <seealso cref="OpenStreamAsync"/>
        protected abstract Task CloseStreamAsync(Stream stream);

        private void OnData(byte[] data, int count)
        {
            var nmea = System.Text.Encoding.UTF8.GetString(data, 0, count);
            List<string> lines = new List<string>();
            lock (m_lockObject)
            {
                m_message += nmea;

                var lineEnd = m_message.IndexOf("\n", StringComparison.Ordinal);
                while (lineEnd > -1)
                {
                    string line = m_message.Substring(0, lineEnd).Trim();
                    m_message = m_message.Substring(lineEnd + 1);
                    if (!string.IsNullOrEmpty(line))
                        lines.Add(line);
                    lineEnd = m_message.IndexOf("\n", StringComparison.Ordinal);
                }
            }
            foreach(var line in lines)
                ProcessMessage(line);
        }

        private IMultiSentenceMessage? _lastMultiMessage;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification="Must silently handle invalid/corrupt input")]
        private void ProcessMessage(string p)
        {
            try
            {
                var msg = NmeaMessage.Parse(p, _lastMultiMessage);
                if(msg is IMultiSentenceMessage multi)
                {
                    if (!multi.IsComplete)
                    {
                        _lastMultiMessage = multi; //Keep it around until next time
                        return;
                    }
                }
                _lastMultiMessage = null;
                if (msg != null)
                    OnMessageReceived(msg);
            }
            catch { }
        }

        private void OnMessageReceived(NmeaMessage msg)
        {
            if (msg == null)
                return;
            
            MessageReceived?.Invoke(this, new NmeaMessageReceivedEventArgs(msg));
        }

        //private readonly Dictionary<string, Dictionary<int, Nmea.NmeaMessage>> MultiPartMessageCache = new Dictionary<string,Dictionary<int,Nmea.NmeaMessage>>();

        /// <summary>
        /// Occurs when an NMEA message is received.
        /// </summary>
        public event EventHandler<NmeaMessageReceivedEventArgs>? MessageReceived;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_stream != null)
            {
                if (m_cts != null)
                {
                    m_cts.Cancel();
                    m_cts = null;
                }
                CloseStreamAsync(m_stream);
                if (disposing && m_stream != null)
                    m_stream.Dispose();
                m_stream = null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this device is open.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is open; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this device supports writing
        /// </summary>
        /// <seealso cref="WriteAsync(byte[], int, int)"/>
        public virtual bool CanWrite { get => false; }

        /// <summary>
        /// Writes to the device stream. Useful for transmitting RTCM corrections to the device
        /// Check the <see cref="CanWrite"/> property before calling this method.
        /// </summary>
        /// <param name="buffer">The byte array that contains the data to write to the port.</param>
        /// <param name="offset">The zero-based byte offset in the buffer parameter at which to begin copying 
        /// bytes to the port.</param>
        /// <param name="length">The number of bytes to write.</param>
        /// <returns>Task</returns>
        /// <seealso cref="CanWrite"/>
        public virtual Task WriteAsync(byte[] buffer, int offset, int length)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Event argument for the <see cref="NmeaDevice.MessageReceived" />
    /// </summary>
    public sealed class NmeaMessageReceivedEventArgs : EventArgs
    {
        internal NmeaMessageReceivedEventArgs(NmeaMessage message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets the nmea message.
        /// </summary>
        /// <value>
        /// The nmea message.
        /// </value>
        public NmeaMessage Message { get; }
    }
}
