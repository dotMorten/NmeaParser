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
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NmeaParser.Gnss.Ntrip
{
    /// <summary>
    /// NTRIP Client for querying an NTRIP server and opening an NTRIP stream
    /// </summary>
    public class Client : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private string? _auth;
        private Socket? sckt;
        private bool connected;
        private Task? runningTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class
        /// </summary>
        /// <param name="host">Host name</param>
        /// <param name="port">Port, usually 2101</param>
        public Client(string host, int port)
        {
            _host = host;
            _port = port;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class
        /// </summary>
        /// <param name="host">Host name</param>
        /// <param name="port">Port, usually 2101</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public Client(string host, int port, string username, string password) : this(host, port)
        {
            _auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));
        }

        /// <summary>
        /// Gets a list of sources from the NTRIP endpoint
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NtripSource> GetSourceTable()
        {
            string data = "";
            byte[] buffer = new byte[1024];
            using (var sck = Request(""))
            {
                int count;
                while ((count = sck.Receive(buffer)) > 0)
                {
                    data += System.Text.Encoding.UTF8.GetString(buffer, 0, count);
                }
            }
            var lines = data.Split('\n');
            List<NtripSource> sources = new List<NtripSource>();
            foreach (var item in lines)
            {
                var d = item.Split(';');
                if (d.Length == 0) continue;
                if (d[0] == "ENDSOURCETABLE")
                    break;
                if (d[0] == "CAS")
                {
                    sources.Add(new Caster(d));
                }
                else if (d[0] == "STR")
                {
                    sources.Add(new NtripStream(d));
                }
            }
            return sources;
        }

        private Socket Request(string path)
        {
            var sckt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sckt.Blocking = true;
            sckt.ReceiveTimeout = 5000;
            sckt.Connect(_host, _port);
            string msg = $"GET /{path} HTTP/1.1\r\n";
            msg += "User-Agent: NTRIP ntripclient\r\n";
            if (_auth != null)
            {
                msg += "Authorization: Basic " + _auth + "\r\n";
            }
            msg += "Accept: */*\r\nConnection: close\r\n";
            msg += "\r\n";

            byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);
            sckt.Send(data);
            return sckt;
        }
        /// <summary>
        /// Connects to the endpoint for the specified <see cref="NtripStream.Mountpoint"/>
        /// </summary>
        /// <param name="stream"></param>
        public void Connect(NtripStream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            Connect(stream.Mountpoint);
        }

        /// <summary>
        /// Connects to the endpoint for the specified <see cref="NtripStream.Mountpoint"/>
        /// </summary>
        /// <param name="mountPoint"></param>
        public void Connect(string mountPoint)
        {
            if (mountPoint == null)
                throw new ArgumentNullException(nameof(mountPoint));
            if (string.IsNullOrWhiteSpace(mountPoint))
                throw new ArgumentException(nameof(mountPoint));
            if (sckt != null) throw new Exception("Connection already open");
            sckt = Request(mountPoint);
            connected = true;
            runningTask = Task.Run(ReceiveThread);
        }

        private async Task ReceiveThread()
        {
            byte[] buffer = new byte[65536];
            
            while (connected && sckt != null)
            {
                int count = sckt.Receive(buffer);
                if (count > 0)
                {
                    DataReceived?.Invoke(this, buffer.Take(count).ToArray());
                }
                await Task.Yield();
                if (!sckt.Connected)
                {
                    if (connected)
                    {
                        connected = false;
                        Disconnected?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                }
            }
            sckt?.Shutdown(SocketShutdown.Both);
            sckt?.Dispose();
            sckt = null;
        }

        /// <summary>
        /// Shuts down the stream
        /// </summary>
        /// <returns></returns>
        public Task CloseAsync()
        {
            if (runningTask != null)
            {
                connected = false;
                var t = runningTask;
                runningTask = null;
                return t;
            }
#if NETSTANDARD || NETFX
            return Task.FromResult<object?>(null);
#else
            return Task.CompletedTask;
#endif
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _ = CloseAsync();
        }

        /// <summary>
        /// Fired when bytes has been received from the stream
        /// </summary>
        public event EventHandler<byte[]>? DataReceived;

        /// <summary>
        /// Fired if the socket connection was dropped, and the connection was closed.
        /// </summary>
        /// <remarks>
        /// This event is useful for handling network glitches, and trying to retry connection by calling <see cref="Connect(string)"/> again a few times.
        /// </remarks>
        public event EventHandler? Disconnected;
    }
}
