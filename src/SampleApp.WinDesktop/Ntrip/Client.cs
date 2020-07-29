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
    public class Client : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private string? _auth;
        private Socket? sckt;
        private bool connected;
        private Task? runningTask;

        public Client(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public Client(string host, int port, string username, string password) : this(host, port)
        {
            _auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));
        }

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

        public void Connect(string strName)
        {
            if (sckt != null) throw new Exception("Connection already open");
            sckt = Request(strName);
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
                await Task.Delay(10);
            }
            sckt?.Shutdown(SocketShutdown.Both);
            sckt?.Dispose();
            sckt = null;
        }

        public Task CloseAsync()
        {
            if (runningTask != null)
            {
                connected = false;
                var t = runningTask;
                runningTask = null;
                return t;
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _ = CloseAsync();
        }

        public event EventHandler<byte[]>? DataReceived;
    }
}
