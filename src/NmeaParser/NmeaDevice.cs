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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace NmeaParser
{
	/// <summary>
	/// A generic abstract NMEA device
	/// </summary>
	public abstract class NmeaDevice : IDisposable
	{
		private object m_lockObject = new object();
		private string m_message = "";
		private Stream m_stream;
		private System.Threading.CancellationTokenSource m_cts;
		private TaskCompletionSource<bool> m_closeTask;
        private bool m_isOpening;

        /// <summary>
        /// Initializes a new instance of the <see cref="NmeaDevice"/> class.
        /// </summary>
        protected NmeaDevice()
		{
		}

        /// <summary>
		/// Opens the device connection.
		/// </summary>
		/// <returns></returns>
		public async Task OpenAsync()
		{
			lock (m_lockObject)
			{
				if (IsOpen || m_isOpening) return;
                m_isOpening = true;
			}
			m_cts = new System.Threading.CancellationTokenSource();
			m_stream = await OpenStreamAsync();
			StartParser();
			MultiPartMessageCache.Clear();
            lock (m_lockObject)
            {
                IsOpen = true;
                m_isOpening = false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "_")]
		private void StartParser()
		{
			var token = m_cts.Token;
			System.Diagnostics.Debug.WriteLine("Starting parser...");
			var _ = Task.Run(async () =>
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
						OnData(buffer.Take(readCount).ToArray());
					}
					await Task.Delay(50);
				}
				if (m_closeTask != null)
					m_closeTask.SetResult(true);
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
        /// Creates the stream the NmeaDevice is working on top off.
        /// </summary>
        /// <returns></returns>
        protected abstract Task<Stream> OpenStreamAsync();

		/// <summary>
		/// Closes the device.
		/// </summary>
		/// <returns></returns>
		public async Task CloseAsync()
		{
			if (m_cts != null)
			{
				m_closeTask = new TaskCompletionSource<bool>();
				if (m_cts != null)
					m_cts.Cancel();
				m_cts = null;
			}
			await m_closeTask.Task;
			await CloseStreamAsync(m_stream);
			MultiPartMessageCache.Clear();
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
		/// <param name="stream">The stream.</param>
		/// <returns></returns>
		protected abstract Task CloseStreamAsync(Stream stream);

		private void OnData(byte[] data)
		{
			var nmea = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification="Must silently handle invalid/corrupt input")]
		private void ProcessMessage(string p)
		{
			try
			{
				var msg = NmeaParser.Nmea.NmeaMessage.Parse(p);
				if (msg != null)
					OnMessageReceived(msg);
			}
			catch { }
		}

		private void OnMessageReceived(Nmea.NmeaMessage msg)
		{
			var args = new NmeaMessageReceivedEventArgs(msg);
			if (msg is IMultiPartMessage multi)
			{
				args.IsMultipart = true;
				if (MultiPartMessageCache.ContainsKey(msg.MessageType))
				{
					var dic = MultiPartMessageCache[msg.MessageType];
					if (dic.ContainsKey(multi.MessageNumber - 1) && !dic.ContainsKey(multi.MessageNumber))
					{
						dic[multi.MessageNumber] = msg;
					}
					else //Something is out of order. Clear cache
						MultiPartMessageCache.Remove(msg.MessageType);
				}
				else if (multi.MessageNumber == 1)
				{
					MultiPartMessageCache[msg.MessageType] = new Dictionary<int, Nmea.NmeaMessage>(multi.TotalMessages);
					MultiPartMessageCache[msg.MessageType][1] = msg;
				}
				if (MultiPartMessageCache.ContainsKey(msg.MessageType))
				{
					var dic = MultiPartMessageCache[msg.MessageType];
					if (dic.Count == multi.TotalMessages) //We have a full list
					{
						MultiPartMessageCache.Remove(msg.MessageType);
						args.MessageParts = dic.Values.ToArray();
                    }
                }
			}

			if (MessageReceived != null)
			{
				MessageReceived(this, args);
			}
		}

		private Dictionary<string, Dictionary<int, Nmea.NmeaMessage>> MultiPartMessageCache
			= new Dictionary<string,Dictionary<int,Nmea.NmeaMessage>>();

		/// <summary>
		/// Occurs when an NMEA message is received.
		/// </summary>
		public event EventHandler<NmeaMessageReceivedEventArgs> MessageReceived;

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
		internal NmeaMessageReceivedEventArgs(Nmea.NmeaMessage message) {
			Message = message;
		}
		/// <summary>
		/// Gets the nmea message.
		/// </summary>
		/// <value>
		/// The nmea message.
		/// </value>
		public Nmea.NmeaMessage Message { get; private set; }
		/// <summary>
		/// Gets a value indicating whether this instance is a multi part message.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is multi part; otherwise, <c>false</c>.
		/// </value>
		public bool IsMultipart { get; internal set; }
		/// <summary>
		/// Gets the message parts if this is a multi-part message and all message parts has been received.
		/// </summary>
		/// <value>
		/// The message parts.
		/// </value>
		public IReadOnlyList<Nmea.NmeaMessage> MessageParts { get; internal set; }
	}
}
