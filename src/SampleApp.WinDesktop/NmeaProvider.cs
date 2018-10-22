using Esri.ArcGISRuntime.Geometry;
using System;
using System.Threading.Tasks;

namespace SampleApp
{
    public class NmeaLocationProvider : Esri.ArcGISRuntime.Location.LocationDataSource
    {
        private NmeaParser.NmeaDevice device;
        double m_Accuracy = 0;
        double m_altitude = double.NaN;
        double m_speed = 0;
        double m_course = 0;

        public NmeaLocationProvider(NmeaParser.NmeaDevice device)
        {
            this.device = device;
            if(device != null)
                device.MessageReceived += device_MessageReceived;
        }
        void device_MessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs e)
        {
            var message = e.Message;
            ParseMessage(message);
        }

        public void ParseMessage(NmeaParser.Nmea.NmeaMessage message)
        {
            bool isNewFix = false;
            bool lostFix = false;
            double lat = 0;
            double lon = 0;
            if (message is NmeaParser.Nmea.Gps.Garmin.Pgrme)
            {
                m_Accuracy = ((NmeaParser.Nmea.Gps.Garmin.Pgrme)message).HorizontalError;
            }
            else if(message is NmeaParser.Nmea.Gst)
            {
                Gst = ((NmeaParser.Nmea.Gst)message);
                m_Accuracy = Math.Sqrt(Gst.SigmaLatitudeError * Gst.SigmaLatitudeError + Gst.SigmaLongitudeError * Gst.SigmaLongitudeError);
            }
            else if(message is NmeaParser.Nmea.Gga)
            {
                Gga = ((NmeaParser.Nmea.Gga)message);
                isNewFix = Gga.Quality != NmeaParser.Nmea.Gps.Gpgga.FixQuality.Invalid;
                lostFix = !isNewFix;
                m_altitude = Gga.Altitude;
                lat = Gga.Latitude;
                lon = Gga.Longitude;
            }
            else if (message is NmeaParser.Nmea.Rmc)
            {
                Rmc = (NmeaParser.Nmea.Rmc)message;
                if (Rmc.Active)
                {
                    isNewFix = true;
                    m_speed = Rmc.Speed;
                    m_course = Rmc.Course;
                    lat = Rmc.Latitude;
                    lon = Rmc.Longitude;
                }
                else lostFix = true;
            }
            else if (message is NmeaParser.Nmea.Gsa)
            {
                Gsa = (NmeaParser.Nmea.Gsa)message;
            }
            if (isNewFix)
            {
                base.UpdateLocation(new Esri.ArcGISRuntime.Location.Location(new MapPoint(lon, lat, m_altitude, SpatialReferences.Wgs84), m_Accuracy, m_speed, m_course, false));
            }
            else if (lostFix)
            {

            }
        }

        protected override Task OnStartAsync()
        {
            if (device != null)
                return this.device.OpenAsync();
            else
                return System.Threading.Tasks.Task<bool>.FromResult(true);
        }

        protected override Task OnStopAsync()
        {
            m_Accuracy = double.NaN;
            if(this.device != null)
                return this.device.CloseAsync();
            else
                return System.Threading.Tasks.Task<bool>.FromResult(true);
        }

        public NmeaParser.Nmea.Gsa Gsa { get; private set; }
        public NmeaParser.Nmea.Gga Gga { get; private set; }
        public NmeaParser.Nmea.Rmc Rmc { get; private set; }
        public NmeaParser.Nmea.Gst Gst { get; private set; }
    }
}
