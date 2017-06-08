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
using System.Threading.Tasks;
using System.IO;
using Windows.Foundation;

namespace NmeaParser
{
	/// <summary>
	/// A generic abstract NMEA device
	/// </summary>
	public abstract class NmeaDevice : IDisposable
	{
		private readonly object m_lockObject = new object();
		private string m_message = "";
		private Stream m_stream;
		private System.Threading.CancellationTokenSource m_cts;
		private TaskCompletionSource<bool> m_closeTask;

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
				if (IsOpen) return;
				IsOpen = true;
			}
			m_cts = new System.Threading.CancellationTokenSource();
			m_stream = await OpenStreamAsync();
			StartParser();
			MultiPartMessageCache.Clear();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "_")]
		private void StartParser()
		{
			var token = m_cts.Token;
			System.Diagnostics.Debug.WriteLine("Starting parser...");
			var _ = Task.Run(async () =>
			{
				var stream = m_stream;
				byte[] buffer = new byte[1024];
				while (!token.IsCancellationRequested)
				{
					int readCount = 0;
					try
					{
						readCount = await stream.ReadAsync(buffer, 0, 1024, token).ConfigureAwait(false);
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
				IsOpen = false;
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
		private void ProcessMessage(string rawMsg)
		{
			try
			{
				var handler = RawMessageReceived;
				if (handler != null)
				{
					handler(this, new RawMessageReceivedEventArgs(rawMsg));
				}

				var msg = Nmea.NmeaMessage.Parse(rawMsg);
				if (msg != null)
					OnMessageReceived(msg);
			}
			catch { }
		}

		private void OnMessageReceived(Nmea.NmeaMessage msg)
		{
			var args = new NmeaMessageReceivedEventArgs(msg);
			var multi = msg as IMultiPartMessage;
			if (multi != null)
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

			var handler = MessageReceived;
			if (handler != null)
			{
				handler(this, args);
			}
		}

		private Dictionary<string, Dictionary<int, Nmea.NmeaMessage>> MultiPartMessageCache
			= new Dictionary<string,Dictionary<int,Nmea.NmeaMessage>>();

		/// <summary>
		/// Occurs when an NMEA message is received.
		/// </summary>
		public event EventHandler<NmeaMessageReceivedEventArgs> MessageReceived;

		/// <summary>
		/// Occurs when an NMEA message is received.
		/// </summary>
		public event EventHandler<RawMessageReceivedEventArgs> RawMessageReceived;

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
	}

	/// <summary>
	/// Event argument for the <see cref="NmeaDevice.RawMessageReceived" />
	/// </summary>
	public sealed class RawMessageReceivedEventArgs : EventArgs
	{
		internal RawMessageReceivedEventArgs(string message)
		{
			Message = message;
		}

		/// <summary>
		/// Gets the raw message.
		/// </summary>
		/// <value>
		/// The raw message.
		/// </value>
		public string Message { get; private set; }
	}

	/// <summary>
	/// Event argument for the <see cref="NmeaDevice.MessageReceived" />
	/// </summary>
	public sealed class NmeaMessageReceivedEventArgs : EventArgs
	{
		internal NmeaMessageReceivedEventArgs(Nmea.NmeaMessage message)
		{
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
