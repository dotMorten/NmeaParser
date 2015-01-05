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
    /// Udp device
    /// </summary>
    public class UdpDevice : NmeaDevice
    {
        UdpClient _client;
        UdpState _state;
        IPEndPoint _endPoint;

        public UdpDevice(string ip, int port)
        {
            _endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            _client = new UdpClient(_endPoint);
            _state = new UdpState() { endPoint = _endPoint, udpClient = _client };
        }

        private void Begin(IAsyncResult ar)
        {
            try
            {
                UdpClient client = (UdpClient)((UdpState)ar.AsyncState).udpClient;
                IPEndPoint wantedIpEndPoint = (IPEndPoint)((UdpState)(ar.AsyncState)).endPoint;
                IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                byte[] receiveBytes = client.EndReceive(ar, ref receivedIpEndPoint);

                // Check sender
                //string receivedText = Encoding.ASCII.GetString(receiveBytes);
                bool isRightHost = (wantedIpEndPoint.Address.Equals(receivedIpEndPoint.Address)
                                   || wantedIpEndPoint.Address.Equals(IPAddress.Any));
                bool isRightPort = (wantedIpEndPoint.Port == receivedIpEndPoint.Port)
                                   || wantedIpEndPoint.Port == 0;

                OnData(receiveBytes);
                client.BeginReceive(Begin, ar.AsyncState);
            }
            catch (Exception e)
            {
                if (!(e is ObjectDisposedException))
                    throw;
            }
        }

        protected override Task<Stream> OpenStreamAsync()
        {
            _client.BeginReceive(Begin, _state);
            return TaskEx.FromResult<Stream>(null); //do nothing
        }

        protected override Task CloseStreamAsync(System.IO.Stream stream)
        {
            return TaskEx.FromResult(true); //do nothing
        }

        protected override void Dispose(bool force)
        {
            _client = null;
        }

        protected override void StartParser()
        {
            if (closeTask != null)
                closeTask.SetResult(true);
        }
    }

    public class UdpState
    {
        public IPEndPoint endPoint;
        public UdpClient udpClient;
    }
}
