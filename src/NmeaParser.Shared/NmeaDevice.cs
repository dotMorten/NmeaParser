﻿//
// Copyright (c) 2014 Morten Nielsen
//
// Licensed under the Microsoft Public License (Ms-PL) (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	http://opensource.org/licenses/Ms-PL.html
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
using System.Threading;

namespace NmeaParser
{
    public abstract class NmeaDevice : IDisposable
    {
        private object m_lockObject = new object();
        private string m_message = "";
        private Stream m_stream;
        private System.Threading.CancellationTokenSource m_cts;
        protected TaskCompletionSource<bool> closeTask;

        protected NmeaDevice()
        { }

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

        protected virtual void StartParser()
        {
            var token = m_cts.Token;
            System.Diagnostics.Debug.WriteLine("Parser started");
            TaskEx.Run(async () =>
            {
                var stream = m_stream;
                byte[] buffer = new byte[1024];
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        int readCount = 0;
                        try
                        {
                            readCount = await stream.ReadAsync(buffer, 0, 1024, token).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            if (e is TaskCanceledException)
                                throw;
                        }

                        if (readCount > 0)
                        {
                            OnData(buffer.Take(readCount).ToArray());
                        }
                        await TaskEx.Delay(10, token);
                    }
                    catch (TaskCanceledException)
                    {
                        System.Diagnostics.Debug.WriteLine("Parse Task was canceled");
                        break;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                if (closeTask != null)
                    closeTask.SetResult(true);
            }, token);
        }

        protected abstract Task<Stream> OpenStreamAsync();

        public async Task CloseAsync()
        {
            if (m_cts != null)
            {
                closeTask = new TaskCompletionSource<bool>();
                if (m_cts != null)
                    m_cts.Cancel();
                m_cts = null;
            }

            if (closeTask != null)
                closeTask.Task.Wait(500);
            System.Diagnostics.Debug.WriteLine("Parser stopped");

            await CloseStreamAsync(m_stream);
            MultiPartMessageCache.Clear();
            m_stream = null;
            lock (m_lockObject)
                IsOpen = false;
        }

        protected abstract Task CloseStreamAsync(Stream stream);

        protected void OnData(byte[] data)
        {
            var nmea = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
            string line = null;
            lock (m_lockObject)
            {
                m_message += nmea;

                var lineEnd = m_message.IndexOf("\n");
                if (lineEnd > -1)
                {
                    line = m_message.Substring(0, lineEnd).Trim();
                    m_message = m_message.Substring(lineEnd + 1);
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
            catch
            {
                System.Diagnostics.Debug.WriteLine("Trouble processing message");
            }
        }

        private void OnMessageReceived(Nmea.NmeaMessage msg)
        {
            var args = new NmeaMessageReceivedEventArgs(msg);
            if (msg is IMultiPartMessage)
            {
                args.IsMultiPart = true;
                var multi = (IMultiPartMessage)msg;
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
            = new Dictionary<string, Dictionary<int, Nmea.NmeaMessage>>();

        public event EventHandler<NmeaMessageReceivedEventArgs> MessageReceived;

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool force)
        {
            if (m_stream != null)
            {
                if (m_cts != null)
                {
                    m_cts.Cancel();
                    m_cts = null;
                }
                CloseStreamAsync(m_stream);
                m_stream = null;
            }
        }

        public bool IsOpen { get; private set; }
    }

    public sealed class NmeaMessageReceivedEventArgs : EventArgs
    {
        internal NmeaMessageReceivedEventArgs(Nmea.NmeaMessage message)
        {
            Message = message;
        }
        public Nmea.NmeaMessage Message { get; private set; }
        public bool IsMultiPart { get; internal set; }
        public Nmea.NmeaMessage[] MessageParts { get; internal set; }
    }
}
