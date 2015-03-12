using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NmeaParser
{
    /// <summary>
    /// Udp device
    /// </summary>
    public class UdpDevice : NmeaDevice
    {
        private UdpClient _udp;
        private IPEndPoint _clientEndPoint;
        private IPEndPoint _receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);

        /// <summary>
        /// Constructor. Binds UDP client to given ip address and port.
        /// </summary>
        /// <param name="ip">ip address</param>
        /// <param name="port">port</param>
        public UdpDevice(string ip, int port)
        {
            _clientEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            _udp = new UdpClient();
            _udp.ExclusiveAddressUse = false;
            _udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udp.Client.Bind(_clientEndPoint);
        }

        /// <summary>
        /// Datagram received
        /// </summary>
        /// <param name="asyncResult">Asynchronous operation result</param>
        private void AsyncCallback(IAsyncResult asyncResult)
        {
            // Return if client is disposed
            if (_udp == null)
                return;

            try
            {
                // Read all bytes
                byte[] receivedBytes = _udp.EndReceive(asyncResult, ref _receiveEndPoint);

                // Pass data to NMEA message parser
                OnData(receivedBytes);

                // Begin listening again
                _udp.BeginReceive(AsyncCallback, asyncResult.AsyncState);
            }
            catch (Exception e)
            {
                // Spit out the exeption and throw, should be catched/logged by "other code"
                System.Diagnostics.Debug.WriteLine(string.Format("AsyncCallback threw exception:", e.Message));
                if (!(e is ObjectDisposedException))
                    throw;
            }
        }

        /// <summary>
        /// Begin receiving datagrams
        /// </summary>
        /// <returns></returns>
        protected override Task<Stream> OpenStreamAsync()
        {
            _udp.BeginReceive(AsyncCallback, _udp);
            return TaskEx.FromResult<Stream>(null);
        }

        protected override Task CloseStreamAsync(Stream stream)
        {
            if (_udp.Client.IsBound)
            {
                _udp.Client.Shutdown(SocketShutdown.Both);
                _udp.Client.Close();
            }
            
            return TaskEx.FromResult(true);
        }

        protected override void Dispose(bool force)
        {
            _udp = null;
        }

        protected override void StartParser()
        {
            if (closeTask != null)
                closeTask.SetResult(true);
        }
    }
}
