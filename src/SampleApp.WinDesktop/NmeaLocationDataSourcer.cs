using Esri.ArcGISRuntime.Geometry;
using System;
using System.Threading.Tasks;

namespace SampleApp.WinDesktop
{
    public class NmeaLocationDataSource : Esri.ArcGISRuntime.Location.LocationDataSource
    {
        private static SpatialReference wgs84_ellipsoidHeight = SpatialReference.Create(4326, 115700);
        private readonly NmeaParser.NmeaDevice m_device;
        private double m_Accuracy = 0;
        private double m_altitude = double.NaN;
        private double m_speed = 0;
        private double m_course = 0;
        private bool m_startStopDevice;
        private bool m_supportGNMessages; // If device detect GN* messages, ignore all other Talker ID
        private bool m_supportGGaMessages; //If device support GGA, ignore RMC for location

        public NmeaLocationDataSource(NmeaParser.NmeaDevice device, bool startStopDevice = true)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            this.m_device = device;
            m_startStopDevice = startStopDevice;
        }

        void device_MessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs e)
        {
            var message = e.Message;
            ParseMessage(message);
        }
        public void ParseMessage(NmeaParser.Messages.NmeaMessage message)
        {
            bool isNewFix = false;
            bool lostFix = false;
            double lat = 0;
            double lon = 0;
            if (message.TalkerId == NmeaParser.Talker.GlobalNavigationSatelliteSystem)
                m_supportGNMessages = true;
            else if(m_supportGNMessages && message.TalkerId != NmeaParser.Talker.GlobalNavigationSatelliteSystem)
                    return; // If device supports combined GN* messages, ignore non-GN messages

            if (message is NmeaParser.Messages.Garmin.Pgrme rme)
            {
                m_Accuracy = rme.HorizontalError;
            }
            else if(message is NmeaParser.Messages.Gst gst)
            {
                Gst = gst;
                int significantDigits = (int)Math.Ceiling(-Math.Log(Math.Min(Gst.SigmaLatitudeError%1, Gst.SigmaLongitudeError%1)));
                m_Accuracy = Math.Round(Math.Sqrt(Gst.SigmaLatitudeError * Gst.SigmaLatitudeError + Gst.SigmaLongitudeError * Gst.SigmaLongitudeError), significantDigits);
            }
            else if (message is NmeaParser.Messages.Rmc rmc)
            {
                Rmc = rmc;
                if (Rmc.Active)
                {
                    m_speed = double.IsNaN(Rmc.Speed) ? 0 : Rmc.Speed;
                    if (!double.IsNaN(Rmc.Course))
                        m_course = Rmc.Course;
                    lat = Rmc.Latitude;
                    lon = Rmc.Longitude;
                }
                else
                {
                    lostFix = true;
                }
                isNewFix = !m_supportGGaMessages;
            }
            else if (message is NmeaParser.Messages.Gga gga)
            {
                m_supportGGaMessages = true;
                if (gga.Quality != NmeaParser.Messages.Gga.FixQuality.Invalid)
                {
                    lat = gga.Latitude;
                    lon = gga.Longitude;
                    m_altitude = gga.Altitude + gga.GeoidalSeparation; //Convert to ellipsoidal height
                }
                if (gga.Quality == NmeaParser.Messages.Gga.FixQuality.Invalid || gga.Quality == NmeaParser.Messages.Gga.FixQuality.Estimated)
                {
                    lostFix = true;
                }
                isNewFix = true;
            }
            else if (message is NmeaParser.Messages.Gsa gsa)
            {
                Gsa = gsa;
            }
            if (isNewFix)
            {
                base.UpdateLocation(new Esri.ArcGISRuntime.Location.Location(
                    !double.IsNaN(m_altitude) ? new MapPoint(lon, lat, m_altitude, wgs84_ellipsoidHeight) : new MapPoint(lon, lat, SpatialReferences.Wgs84),
                    m_Accuracy, m_speed, m_course, lostFix));
            }
        }

        protected override Task OnStartAsync()
        {
            m_device.MessageReceived += device_MessageReceived;
            if (m_startStopDevice)
                return this.m_device.OpenAsync();
            else
                return System.Threading.Tasks.Task<bool>.FromResult(true);
        }

        protected override Task OnStopAsync()
        {
            m_device.MessageReceived -= device_MessageReceived;
            m_Accuracy = double.NaN;
            if(m_startStopDevice)
                return this.m_device.CloseAsync();
            else
                return System.Threading.Tasks.Task<bool>.FromResult(true);
        }

        public NmeaParser.Messages.Gsa Gsa { get; private set; }
        public NmeaParser.Messages.Gga Gga { get; private set; }
        public NmeaParser.Messages.Rmc Rmc { get; private set; }
        public NmeaParser.Messages.Gst Gst { get; private set; }
    }
}
