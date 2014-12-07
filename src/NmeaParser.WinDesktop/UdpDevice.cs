using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser
{
    /// <summary>
    /// Udp NMEA device
    /// </summary>
    public class UdpDevice : NmeaDevice
    {
        private UdpClient _client;
        private UdpState _state;
        private IPEndPoint _endPoint;
        private bool _listen;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ip">ip address string</param>
        /// <param name="port">port number</param>
        public UdpDevice(string ip, int port)
            : this(new IPEndPoint(IPAddress.Parse(ip), port))
        {}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ipAddress">ip address</param>
        /// <param name="port">port number</param>
        public UdpDevice(IPAddress ipAddress, int port)
            :this(new IPEndPoint(ipAddress, port))
        {}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ipEndPoint">ip endpoint</param>
        public UdpDevice(IPEndPoint ipEndPoint)
        {
            _endPoint = ipEndPoint;

            _client = new UdpClient(_endPoint);
            _state = new UdpState() { EndPoint = _endPoint, UdpClient = _client };
        }

        /// <summary>
        /// Specify desired endpoint if datagrams are received from multiple sources
        /// </summary>
        public IPEndPoint ExpectedEndpoint { get; set; }

        /// <summary>
        /// Override. Set closeTask result as true since we are not using base's parser
        /// </summary>
        protected override void StartParser()
        {
            closeTask = new TaskCompletionSource<bool>();
            closeTask.SetResult(true);
        }

        /// <summary>
        /// Override. Start listening and attach async callback.
        /// </summary>
        /// <returns>completed task resolved as null stream</returns>
        protected override Task<Stream> OpenStreamAsync()
        {
            _listen = true;
            _client.BeginReceive(Callback, _state);
            return Task.FromResult<Stream>(null);
        }

        /// <summary>
        /// Override. Stop listening.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>completed task resolved to true</returns>
        protected override Task CloseStreamAsync(System.IO.Stream stream)
        {
            _listen = false;
            return Task.FromResult(true);
        }

        /// <summary>
        /// "Dispose" udp client.
        /// </summary>
        protected override void Dispose(bool force)
        {
            _client = null;
        }

        /// <summary>
        /// Handle received datagrams async
        /// </summary>
        /// <param name="ar"></param>
        private void Callback(IAsyncResult ar)
        {
            var client = (UdpClient)((UdpState)ar.AsyncState).UdpClient;
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] bytes = client.EndReceive(ar, ref endPoint);
            
            if (!_listen)
                return;

            if (ExpectedEndpoint == null || (ExpectedEndpoint.Address.Equals(endPoint.Address) && ExpectedEndpoint.Port.Equals(endPoint.Port)))
                OnData(bytes); // parse
            client.BeginReceive(Callback, ar.AsyncState);
        }
    }

    /// <summary>
    /// Udp helper class.
    /// http://msdn.microsoft.com/en-us/library/c8s04db1(v=VS.80).aspx
    /// </summary>
    public class UdpState
    {
        /// <summary>
        /// IPEndPoint
        /// </summary>
        public IPEndPoint EndPoint;

        /// <summary>
        /// UdpClient
        /// </summary>
        public UdpClient UdpClient;
    }
}
