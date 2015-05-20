using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NLog;

namespace NmeaParser
{
    /// <summary>
    /// Udp device
    /// </summary>
    public class UdpDevice : NmeaDevice
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const int VistaVersion = 6;
        private readonly int? _osVersion;
        private volatile object _threadLock = new object();
        private UdpClient _udp;
        private IPEndPoint _receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);

        /// <summary>
        /// Bind UDP client to given ip address and port.
        /// </summary>
        /// <param name="ip">ip address</param>
        /// <param name="port">port</param>
        public UdpDevice(string ip, int port)
        {
            try
            {
                Logger.Info("New UDP device instantiated, binding to {0}:{1}", ip, port);
                _osVersion = Environment.OSVersion.Version.Major;
                var clientEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

                _udp = new UdpClient();
                _udp.ExclusiveAddressUse = false;
                _udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udp.Client.Bind(clientEndPoint);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        /// <summary>
        /// Datagram received callback
        /// </summary>
        /// <param name="asyncResult">Async operation result</param>
        private void AsyncCallback(IAsyncResult asyncResult)
        {
            lock (_threadLock)
            {
                if (_udp == null)
                    return;

                try
                {
                    // Read all bytes
                    byte[] receivedBytes = _udp.EndReceive(asyncResult, ref _receiveEndPoint);
                    
                    // Begin listening again
                    BeginReceiveAsync();

                    // Pass received data to NMEA message parser
                    OnData(receivedBytes);
                }
                catch (Exception e)
                {
                    if (!(e is ObjectDisposedException) && !(e is SocketException))
                    {
                        Logger.Error(e);
                        throw;
                    }
                    Logger.Warn(e);
                }
            }
        }

        /// <summary>
        /// Receive datagram from remote host asynchronously. 
        /// </summary>
        /// <remarks>
        /// Separate logic for pre- and post Windows Vista operating systems
        /// </remarks>
        private void BeginReceiveAsync()
        {
            if (_osVersion <= VistaVersion)
                TaskEx.Run(() => _udp.BeginReceive(AsyncCallback, null)).ConfigureAwait(false);
            else
                _udp.BeginReceive(AsyncCallback, null);
        }

        /// <summary>
        /// Begin receiving datagrams
        /// </summary>
        /// <returns></returns>
        protected override Task<Stream> OpenStreamAsync()
        {
            try
            {
                Logger.Info("Open UDP device");
                BeginReceiveAsync();
                return TaskEx.FromResult<Stream>(null);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        protected override Task CloseStreamAsync(Stream stream)
        {
            try
            {
                Logger.Info("Close UDP device");
                if (_udp.Client.IsBound)
                {
                    _udp.Client.Shutdown(SocketShutdown.Both);
                    _udp.Client.Close();
                }
                return TaskEx.FromResult(true);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        protected override void Dispose(bool force)
        {
            Logger.Info("Dispose UDP device");
            _udp = null;
        }

        protected override void StartParser()
        {
            if (closeTask != null)
                closeTask.SetResult(true);
        }
    }
}
