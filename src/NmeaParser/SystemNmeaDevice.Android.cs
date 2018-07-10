#if __ANDROID__
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.Locations;
using Android.App;
using Android.OS;
using System.Globalization;

namespace NmeaParser
{
    /// <summary>
    /// This class provides access to the Android system location service's raw NMEA stream
    /// The <c>ACCESS_FINE_LOCATION</c> permission is required.
    /// </summary>
    public class SystemNmeaDevice : NmeaDevice
    {
        private StringStream stream;
        private Listener listener;
        private LocationManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemNmeaDevice"/> class.
        /// </summary>
        public SystemNmeaDevice()
        {
            manager = Application.Context.GetSystemService(Context.LocationService) as LocationManager;
        }

        /// <summary>
        /// Gets the estimated accuracy of this location, in meters as reported by the system location provider (not NMEA based).
        /// Returns NaN if no estimate is available or the active location provider isn't GPS (ie IP/WiFi/Network Provider)
        /// </summary>
        /// <remarks>
        /// Normally the **GST messages will provide the estimated accuracy, but this is not always exposed as NMEA
        /// on Android devices, so this serves as a useful fallback.
        /// </remarks>
        public float Accuracy => listener == null ? float.NaN : listener.Accuracy;

        /// <inheritdoc />
        [Android.Runtime.RequiresPermission("android.permission.ACCESS_FINE_LOCATION")]
        protected override Task<Stream> OpenStreamAsync()
        {
            if(!manager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                throw new InvalidOperationException("The GPS Location Provider is not enabled");
            }

            stream = new StringStream();
            listener = new Listener();
            listener.NmeaMessage += (s, e) => stream?.Append(e);
            bool success = manager.AddNmeaListener(listener);
            manager.RequestLocationUpdates(LocationManager.GpsProvider, 0, 0f, listener );
            return Task.FromResult<Stream>(stream);
        }

        /// <inheritdoc />
        protected override Task CloseStreamAsync(Stream stream)
        {
            manager.RemoveUpdates(listener);
            manager.RemoveNmeaListener(listener);
            listener.Dispose();
            listener = null;
            stream.Dispose();
            return Task.FromResult<object>(null);
        }

        private class Listener : Java.Lang.Object,
#if API_LEVEL_24
            IOnNmeaMessageListener,
#else
            GpsStatus.INmeaListener,
#endif
            ILocationListener
        {
            private bool _isNmeaSupported = false;

#if API_LEVEL_24
            void IOnNmeaMessageListener.OnNmeaMessage(string message, long timestamp)
#else
            void GpsStatus.INmeaListener.OnNmeaReceived(long timestamp, string message)
#endif
            {
                _isNmeaSupported = true;
                NmeaMessage?.Invoke(this, message);
            }

            public event EventHandler<string> NmeaMessage;

            public float Accuracy = float.NaN;

            void ILocationListener.OnLocationChanged(Location location)
            {
                if (location.Provider != LocationManager.GpsProvider)
                {
                    Accuracy = float.NaN;
                    return;
                }
                Accuracy = location.HasAccuracy ? location.Accuracy : float.NaN;
                if (_isNmeaSupported) return;
                // Not all Android devices support reporting NMEA, so we'll fallback to just generating
                // simple RMC and GGA message so the provider continues to work across multiple devices
                // $GPRMC:
                List<string> values = new List<string>(12);
                values.Add("$GPRMC");
                DateTimeOffset d = DateTimeOffset.FromUnixTimeMilliseconds(location.Time);
                values.Add(d.ToString("hhmmss"));
                values.Add("A");
                var lat = Math.Floor(Math.Abs(location.Latitude));
                var latfrac = (Math.Abs(location.Latitude) - lat) * 60;
                values.Add($"{lat.ToString("00")}{latfrac.ToString(CultureInfo.InvariantCulture)}");
                values.Add(location.Latitude < 0 ? "S" : "N");
                var lon = Math.Floor(Math.Abs(location.Longitude));
                var lonfrac = (Math.Abs(location.Longitude) - lon) * 60;
                values.Add($"{lon.ToString("000")}{lonfrac.ToString(CultureInfo.InvariantCulture)}");
                values.Add(location.Longitude < 0 ? "W" : "E");
                values.Add(location.HasSpeed ? location.Speed.ToString(CultureInfo.InvariantCulture) : "");
                values.Add(location.HasBearing ? location.Bearing.ToString(CultureInfo.InvariantCulture) : "");
                values.Add(d.ToString("ddMMyy"));
                values.Add(""); //Variation
                values.Add(""); //East/West
                NmeaMessage?.Invoke(this, string.Join(",", values) + "\n");
                // $GPGGA:
                int satellites = 0;
                if(location.Extras != null && location.Extras.ContainsKey("satellites"))
                {
                    satellites = Convert.ToInt32(location.Extras.Get("satellites"));
                }
                values = new List<string>(13);
                values.Add("$GPGGA");
                values.Add(d.ToString("hhmmss"));
                values.Add($"{lat.ToString("00")}{latfrac.ToString(CultureInfo.InvariantCulture)}");
                values.Add(location.Latitude < 0 ? "S" : "N");
                values.Add($"{lon.ToString("000")}{lonfrac.ToString(CultureInfo.InvariantCulture)}");
                values.Add(location.Longitude < 0 ? "W" : "E");
                values.Add("1"); //Fix Quality:
                values.Add(satellites.ToString()); //Number of Satellites
                values.Add(""); //HDOP
                values.Add(location.HasAltitude ? location.Altitude.ToString(CultureInfo.InvariantCulture) : "");
                values.Add("M"); //Altitude units
                values.Add(""); //Height of geoid above WGS84 ellipsoid
                values.Add(""); //Geoid height units
                values.Add(""); //Time since last DGPS update
                values.Add(""); //DGPS reference station id
                NmeaMessage?.Invoke(this, string.Join(",", values) + "\n");
            }

            void ILocationListener.OnProviderDisabled(string provider) { }

            void ILocationListener.OnProviderEnabled(string provider) { }

            void ILocationListener.OnStatusChanged(string provider, Availability status, Bundle extras) { }
        }

        /// <summary>
        /// Class for converting streaming strings into a stream
        /// </summary>
        private class StringStream : Stream
        {
            object lockObject = new object();
            Queue<byte> stream = new Queue<byte>();
            long position = 0;
            public StringStream() { }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => throw new NotImplementedException();

            public override long Position { get => position; set => throw new NotSupportedException(); }

            public void Append(string data)
            {
                lock (lockObject)
                {
                    foreach (byte b in Encoding.UTF8.GetBytes(data))
                    {
                        stream.Enqueue(b);
                    }
                }
            }

            public override void Flush() { }

            public override int Read(byte[] buffer, int offset, int count)
            {
                lock (lockObject)
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (stream.Count == 0)
                            return i;
                        buffer[offset + i] = stream.Dequeue();
                    }
                }
                return count;
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
#endif