using NmeaParser.Nmea;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NmeaParser
{
    public class GnssMessageProcessor : MessageAggregator
    {

        public GnssMessageProcessor(NmeaDevice device) : base(device)
        {
        }

        protected override void OnMessagesReceived(NmeaMessage[] messages)
        {
            base.OnMessagesReceived(messages);
            // We prefer GNSS messages, so put thos first
            var info = new PositionInformation();
            messages = messages.OrderBy(rmc => rmc.TalkerId == Talker.GlobalNavigationSatelliteSystem ? -100 : (int)rmc.TalkerId).ToArray();
            info.Longitude = double.NaN;
            info.Latitude = double.NaN;
            var RmcMessages = messages.OfType<Rmc>().Where(rmc => rmc.Active);
            if (RmcMessages.Any())
            {
                var rmc = RmcMessages.First();
                info.Latitude = rmc.Latitude;
                info.Longitude = rmc.Longitude;
                info.Speed = rmc.Speed;
                info.TimeOfFix = rmc.FixTime;
            }
            var GsaMessages = messages.OfType<Gsa>().Where(gsa => gsa.Fix == Gsa.FixType.Fix3D);
            if (!GsaMessages.Any())
                GsaMessages = messages.OfType<Gsa>().Where(gsa => gsa.Fix == Gsa.FixType.Fix2D);
            if (GsaMessages.Any())
            {
                var gsa = GsaMessages.First();
                info.Hdop = gsa.Hdop;
                info.Pdop = gsa.Pdop;
                info.Vdop = gsa.Vdop;
                info.SatelliteIds = gsa.SatelliteIDs;
            }
            var GsvMessages = messages.OfType<Gsv>();
            if (GsvMessages.Any())
            {
                Gsv gsv = GsvMessages.First();
                info.Satellites = gsv.SVs.ToArray();
            }
            var GstMessages = messages.OfType<Gst>();
            if (GstMessages.Any())
            {
                Gst gst = GstMessages.First();
                info.HorizontalAccuracy = (gst.SemiMajorError + gst.SemiMinorError) / 2;
                info.VerticalAccuracy = gst.SigmaHeightError;
                if (!info.TimeOfFix.HasValue)
                {
                    info.TimeOfFix = DateTime.UtcNow.Date.Add(gst.FixTime);
                }
            }
            var GgaMessages = messages.OfType<Gga>().Where(g=>g.Quality != Gga.FixQuality.Invalid);
            if (GgaMessages.Any())
            {
                Gga gga = GgaMessages.First();
                info.Latitude = gga.Latitude;
                info.Longitude = gga.Longitude;
                info.Altitude = gga.Altitude;
                info.GeoidalSeparation = gga.GeoidalSeparation;
                if (!info.TimeOfFix.HasValue)
                {
                    info.TimeOfFix = DateTime.UtcNow.Date.Add(gga.FixTime);
                }
            }
            var GnsMessages = messages.OfType<Gns>();
            if (GnsMessages.Any())
            {
                Gns gns = GnsMessages.First();
                if (!info.TimeOfFix.HasValue)
                {
                    info.TimeOfFix = DateTime.UtcNow.Date.Add(gns.FixTime);
                }
                info.GeoidalSeparation = gns.GeoidalSeparation;
                info.Latitude = gns.Latitude;
                info.Longitude = gns.Longitude;
            }
            LocationUpdated?.Invoke(this, info);
        }

        public event EventHandler<PositionInformation> LocationUpdated;
    }

    public struct PositionInformation
    { 
        public double? Longitude { get; internal set; } 
        public double? Latitude { get; internal set; }
        public double? Altitude { get; internal set; }
        public double? GeoidalSeparation { get; internal set; }
        public double? Speed { get; internal set; }
        public double? Pdop { get; internal set; }
        public double? Hdop { get; internal set; }
        public double? Vdop { get; internal set; }
        public double? HorizontalAccuracy { get; internal set; }
        public double? VerticalAccuracy { get; internal set; }
        public DateTimeOffset? TimeOfFix { get; internal set; }
        public int[] SatelliteIds { get; internal set; }
        public SatelliteVehicle[] Satellites { get; internal set; }
    }
}
