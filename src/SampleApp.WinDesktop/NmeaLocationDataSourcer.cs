using Esri.ArcGISRuntime.Geometry;
using System;
using System.Threading.Tasks;

namespace SampleApp.WinDesktop
{
    public class NmeaLocationDataSource : Esri.ArcGISRuntime.Location.LocationDataSource
    {
        private NmeaParser.NmeaDevice device;
        private double m_Accuracy = 0;
        private double m_altitude = double.NaN;
        private double m_speed = 0;
        private double m_course = 0;
        private bool supportGNMessages; // If device detect GN* messages, ignore all other Talker ID
        private bool supportGGaMessages; //If device support GGA, ignore RMC for location

        public NmeaLocationDataSource(NmeaParser.NmeaDevice device)
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
        public void ParseMessage(NmeaParser.Messages.NmeaMessage message)
        {
            bool isNewFix = false;
            bool lostFix = false;
            double lat = 0;
            double lon = 0;
            if (message.TalkerId == NmeaParser.Talker.GlobalNavigationSatelliteSystem)
                supportGNMessages = true;
            else if(supportGNMessages && message.TalkerId != NmeaParser.Talker.GlobalNavigationSatelliteSystem)
                    return; // If device supports combined GN* messages, ignore non-GN messages

            if (message is NmeaParser.Messages.Garmin.Pgrme rme)
            {
                m_Accuracy = rme.HorizontalError;
            }
            else if(message is NmeaParser.Messages.Gst gst)
            {
                Gst = gst;
                m_Accuracy = Math.Sqrt(Gst.SigmaLatitudeError * Gst.SigmaLatitudeError + Gst.SigmaLongitudeError * Gst.SigmaLongitudeError);
            }
            else if (message is NmeaParser.Messages.Rmc rmc)
            {
                Rmc = rmc;
                if (Rmc.Active)
                {
                    m_speed = double.IsNaN(Rmc.Speed) ? Rmc.Speed : 0;
                    if (!double.IsNaN(Rmc.Course))
                        m_course = Rmc.Course;
                    lat = Rmc.Latitude;
                    lon = Rmc.Longitude;
                }
                else
                {
                    lostFix = true;
                }
                isNewFix = !supportGGaMessages;
            }
            else if (message is NmeaParser.Messages.Gga gga)
            {
                supportGGaMessages = true;
                if (gga.Quality != NmeaParser.Messages.Gga.FixQuality.Invalid)
                {
                    lat = Rmc.Latitude;
                    lon = Rmc.Longitude;
                    m_altitude = gga.Altitude + gga.GeoidalSeparation; //Convert to ellipsoidal height
                }
                if (gga.Quality != NmeaParser.Messages.Gga.FixQuality.Invalid || gga.Quality == NmeaParser.Messages.Gga.FixQuality.Estimated)
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
        private static SpatialReference wgs84_ellipsoidHeight = SpatialReference.Create(4326, 115700);
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

        public NmeaParser.Messages.Gsa Gsa { get; private set; }
        public NmeaParser.Messages.Gga Gga { get; private set; }
        public NmeaParser.Messages.Rmc Rmc { get; private set; }
        public NmeaParser.Messages.Gst Gst { get; private set; }
    }
}
