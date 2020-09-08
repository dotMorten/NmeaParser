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
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NmeaParser.Gnss.Ntrip
{
    /// <summary>
    /// NTRIP Client for querying an NTRIP server and opening an NTRIP stream
    /// </summary>
    public class Client
    {
        private readonly string _host;
        private readonly int _port;
        private string? _auth;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class
        /// </summary>
        /// <param name="host">Host name</param>
        /// <param name="port">Port, usually 2101</param>
        public Client(string host, int port)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));
            _host = host;
            if (port < 1)
                throw new ArgumentOutOfRangeException(nameof(port));
            _port = port;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class
        /// </summary>
        /// <param name="host">Host name</param>
        /// <param name="port">Port, usually 2101</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public Client(string host, int port, string? username, string? password) : this(host, port)
        {
            if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
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
        public Stream OpenStream(NtripStream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            return OpenStream(stream.Mountpoint);
        }

        /// <summary>
        /// Connects to the endpoint for the specified <see cref="NtripStream.Mountpoint"/>
        /// </summary>
        /// <param name="mountPoint"></param>       
        public Stream OpenStream(string mountPoint)
        {
            if (mountPoint == null)
                throw new ArgumentNullException(nameof(mountPoint));
            if (string.IsNullOrWhiteSpace(mountPoint))
                throw new ArgumentException(nameof(mountPoint));
            
            return new NtripDataStream(() => Request(mountPoint));
        }

        private class NtripDataStream : System.IO.Stream
        {
            private Func<Socket> m_openSocketAction;
            private Socket m_socket;

            public NtripDataStream(Func<Socket> openSocketAction)
            {
                m_openSocketAction = openSocketAction;
                m_socket = openSocketAction();
            }

            public override bool CanRead => m_socket.Connected;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => -1;

            long position = 0;
            public override long Position { get => position; set => throw new NotSupportedException(); }

            public override void Flush() => throw new NotSupportedException();

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (isDiposed)
                    throw new ObjectDisposedException("NTRIP Stream");
                if(!m_socket.Connected)
                {
                    // reconnect
                    m_socket.Dispose();
                    m_socket = m_openSocketAction();
                }
                int read = m_socket.Receive(buffer, offset, count, SocketFlags.None);
                position += read;
                return read;
            }
#if !NETSTANDARD1_4
            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
                if (cancellationToken.CanBeCanceled)
                    cancellationToken.Register(() => tcs.TrySetCanceled());
                if (isDiposed)
                    throw new ObjectDisposedException("NTRIP Stream");
                if (!m_socket.Connected)
                {
                    // reconnect
                    m_socket.Dispose();
                    m_socket = m_openSocketAction();
                }
                m_socket.BeginReceive(buffer, offset, count, SocketFlags.None, ReceiveCallback, tcs);
                return tcs.Task;
            }

            private void ReceiveCallback(IAsyncResult ar)
            {
                TaskCompletionSource<int> tcs = (TaskCompletionSource<int>)ar.AsyncState;
                if (tcs.Task.IsCanceled) return;
                try
                {
                    int bytesRead = m_socket.EndReceive(ar);
                    position += bytesRead;
                    tcs.TrySetResult(bytesRead);
                }
                catch (System.Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }
#endif
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            protected override void Dispose(bool disposing)
            {
                isDiposed = true;
                m_socket.Dispose();
                base.Dispose(disposing);
            }
            private bool isDiposed;
            public override int ReadTimeout { get => m_socket.ReceiveTimeout; set => m_socket.ReceiveTimeout = value; }
        }
    }
}
