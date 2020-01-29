#Using NTRIP for DGPS

You can use data from a nearby NTRIP server to improve the accuracy if your GPS position, if your device supports it.
The Serial and Bluetooth devices supports writing to them, so you merely need to stream the data from the NTRIP server directly to your device.

We'll first create a simple NTRIP client library:

```cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ntrip
{
    public abstract class NtripSource
    {
    }

    public class Caster : NtripSource
    {
        public IPAddress Address { get; set; }
        public int Port { get; set; }
        public string Identifier { get; set; }
        public string Operator { get; set; }
        public bool SupportsNmea { get; set; }
        public string CountryCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public IPAddress FallbackAddress { get; set; }
    }

    public class NtripStream : NtripSource
    {
        public string Mountpoint { get; set; }
        public string Identifier { get; set; }
        public string Format { get; set; }
        public string FormatDetails { get; set; }
        public Carrier Carrier { get; set; }
        public string Network { get; set; }
        public string CountryCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool SupportsNmea { get; set; }
    }

    public enum Carrier : int
    {
        No = 0,
        L1 = 1,
        L1L2 = 2
    }

    public class Client : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private string _auth;
        private Socket sckt;
        public Client(string host, int port, string authKey = null)
        {
            _host = host;
            _port = port;
            _auth = authKey;
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
                    var c = new Caster();
                    var a = d[1].Split(':');
                    c.Address = IPAddress.Parse(a[0]);
                    c.Port = int.Parse(a[1]);
                    c.Identifier = d[3];
                    c.Operator = d[4];
                    c.SupportsNmea = d[5] == "1";
                    c.CountryCode = d[6];
                    c.Latitude = double.Parse(d[7], CultureInfo.InvariantCulture);
                    c.Longitude = double.Parse(d[8], CultureInfo.InvariantCulture);
                    c.FallbackAddress = IPAddress.Parse(d[9]);
                    sources.Add(c);
                }
                else if (d[0] == "STR")
                {
                    var str = new NtripStream();
                    str.Mountpoint = d[1];
                    str.Identifier = d[2];
                    str.Format = d[3];
                    str.FormatDetails = d[4];
                    str.Carrier = (Carrier)int.Parse(d[5]);
                    str.Network = d[7];
                    str.CountryCode = d[8];
                    str.Latitude = double.Parse(d[9], CultureInfo.InvariantCulture);
                    str.Longitude = double.Parse(d[10], CultureInfo.InvariantCulture);
                    str.SupportsNmea = d[11] == "1";
                    sources.Add(str);
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

        private bool connected;
        private Task runningTask;
        private async Task ReceiveThread()
        {
            byte[] buffer = new byte[65536];

            while (connected)
            {
                int count = sckt.Receive(buffer);
                if(count > 0)
                {
                    DataReceived?.Invoke(this, buffer.Take(count).ToArray());
                }
                await Task.Delay(10);
            }
            sckt.Shutdown(SocketShutdown.Both);
            sckt.Close();
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

        public event EventHandler<byte[]> DataReceived;
    }
}
```

You can next connect to your NTRIP server, pick a stream and start streaming it to your device.

Example:

```cs
string hostname = "esricaster.esri.com"; // Replace with a server near you
int port = 2101; //Port for the ntrip server
var client = new Ntrip.Client(hostname, port);
// Get the source table from the server:
var table = client.GetSourceTable();
// Just pick the first Ntrip datastream:
var str = table.OfType<NtripStream>().First();
// Listen for data, and simply forward it to the NMEA device:
client.DataReceived += (sender, ntripData) => {
   nmeaDevice.WriteAsync(ntripData, 0, ntripData.Length);   
};
// Connect to the stream
client.Connect(str.Mountpoint);
```