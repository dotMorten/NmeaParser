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
using Windows.Foundation;

namespace NmeaParser
{
	public abstract class NmeaDevice : IDisposable
	{
		private object lockObject = new object();
		private string message = "";
		private Stream m_stream;
		System.Threading.CancellationTokenSource tcs;
		TaskCompletionSource<bool> closeTask;

		protected NmeaDevice()
		{
		}
		public async Task OpenAsync()
		{
			tcs = new System.Threading.CancellationTokenSource();
			m_stream = await OpenStreamAsync();
			StartParser();
		}

		private void StartParser()
		{
			var token = tcs.Token;
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
					await Task.Delay(10, token);
				}
				if (closeTask != null)
					closeTask.SetResult(true);
			});
		}

		protected abstract Task<Stream> OpenStreamAsync();
		public async Task CloseAsync()
		{
			if (tcs != null)
			{
				closeTask = new TaskCompletionSource<bool>();
				if (tcs != null)
					tcs.Cancel();
				tcs = null;
			}
			await closeTask.Task;
			await CloseStreamAsync(m_stream);
			m_stream = null;
		}
		protected abstract Task CloseStreamAsync(Stream stream);

		private void OnData(byte[] data)
		{
			var nmea = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
			string line = null;
			lock (lockObject)
			{
				message += nmea;

				var lineEnd = message.IndexOf("\n");
				if (lineEnd > -1)
				{
					line = message.Substring(0, lineEnd).Trim();
					message = message.Substring(lineEnd).Trim();
				}
			}
			if (!string.IsNullOrEmpty(line))
				ProcessMessage(line);
		}

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
			if (MessageReceived != null)
				MessageReceived(this, msg);
		}

		public event TypedEventHandler<NmeaDevice, Nmea.NmeaMessage> MessageReceived;

		public void Dispose()
		{
			Dispose(true);
		}
		protected virtual void Dispose(bool force)
		{
			if (m_stream != null)
			{
				if (tcs != null)
				{
					tcs.Cancel();
					tcs = null;
				}
				CloseStreamAsync(m_stream);
				m_stream = null;
			}
		}
	}
}
