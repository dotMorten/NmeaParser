# Creating a location provider for ArcGIS Runtime SDK

Below is an implementation for use with the [ArcGIS Runtime SDK for .NET](http://developers.arcgis.com/net). Use this location provider on the MapView's LocationDisplay to send it location data from your NMEA device to display your current location on a map.

Below is an example implementation of this.
You can also check out the Desktop Sample app in the [Github Repo]( https://github.com/dotMorten/NmeaParser/blob/main/src/SampleApp.WinDesktop/NmeaProvider.cs) which uses this to display a map.

**Usage:**
```csharp
NmeaParser.NmeaDevice device = new NmeaParser.NmeaFileDevice("NmeaSampleData.txt");
mapView.LocationDisplay.DataSource = new NmeaLocationProvider(device);
mapView.LocationDisplay.InitialZoomScale = 20000;
mapView.LocationDisplay.IsEnabled = true;
```

**NmeaLocationProvider.cs**
```csharp
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using NmeaParser;

namespace NmeaParser.ArcGIS
{
    public class NmeaLocationProvider : LocationDataSource
    {
        private readonly NmeaParser.NmeaDevice device;

        public NmeaLocationProvider(NmeaParser.NmeaDevice device)
        {
            this.device = device;
            device.MessageReceived += NmeaMessageReceived;
        }

        private void NmeaMessageReceived(object sender, NmeaMessageReceivedEventArgs e)
        {
            var message = e.Message;
            if (message is NmeaParser.Messages.Rmc rmc && rmc.Active)
            {
                base.UpdateLocation(new Location(
                    new MapPoint(rmc.Longitude, rmc.Latitude, SpatialReferences.Wgs84),
                    horizontalAccuracy: double.NaN,
                    velocity: double.IsNaN(rmc.Speed) ? 0 : rmc.Speed,
                    course: double.IsNaN(rmc.Course) ? 0 : rmc.Course, // Current ArcGIS Runtime limitation that course can't be NaN
                    isLastKnown: false));
            }
        }
        protected override Task OnStartAsync() => device.OpenAsync();

        protected override Task OnStopAsync() => device.CloseAsync();
    }
}

```

### Combining multiple NMEA messages into a single location event
NMEA often happens in a burst of messages, which could be combined to one larger location object with more information available.
By relying on the time stamp in most of the messages, we can combine them all to get better metadata about the location.
```
using System;
using System.Linq;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using NmeaParser;
using NmeaParser.Messages;

namespace NmeaParser.ArcGIS
{
    public class NmeaLocationProvider : LocationDataSource
    {
        private readonly NmeaParser.NmeaDevice device;
        private Gga lastGga;
        private Rmc lastRmc;
        private Gsa lastGsa;
        private Gst lastGst;

        public NmeaLocationProvider(NmeaParser.NmeaDevice device)
        {
            this.device = device;
            device.MessageReceived += NmeaMessageReceived;
        }
        private void NmeaMessageReceived(object sender, NmeaMessageReceivedEventArgs e)
        {
            var message = e.Message;
            bool newFix = false;
            if (message is Rmc rmc && rmc.Active)
            {
                lastRmc = rmc;
                newFix = true;
            }
            else if (message is Gga gga)
            {
                lastGga = gga;
                newFix = true;
            }
            else if (message is Gst gst)
            {
                lastGst = gst;
                newFix = true;
            }
            else if (message is Gsa gsa)
            {
                lastGsa = gsa;
            }
            else
            {
                return;
            }
            // We require the timestamps to match to raise them together. Gsa doesn't have a time stamp so just using latest for that
            TimeSpan? timeOfFixMax = MaxTime(lastRmc?.FixTime.TimeOfDay, lastGga?.FixTime, lastGst?.FixTime);
            TimeSpan? timeOfFixMin = MinTime(lastRmc?.FixTime.TimeOfDay, lastGga?.FixTime, lastGst?.FixTime);
            if (newFix && timeOfFixMax == timeOfFixMin)
            {
                var location = NmeaLocation.Create(timeOfFixMax.Value, lastRmc, lastGga, lastGsa, lastGst);
                if (location != null)
                    base.UpdateLocation(location);
            }
        }
        private static TimeSpan? MaxTime(params TimeSpan?[] timeSpans) => timeSpans.Where(t => t != null).Max();
        private static TimeSpan? MinTime(params TimeSpan?[] timeSpans) => timeSpans.Where(t => t != null).Min();
        protected override Task OnStartAsync() => device.OpenAsync();

        protected override Task OnStopAsync() => device.CloseAsync();
    }

    /// <summary>
    /// Custom location class with the additional NMEA information associated with it
    /// </summary>
    public class NmeaLocation : Location
    {
        private NmeaLocation(DateTimeOffset? timestamp, MapPoint position, double horizontalAccuracy, double verticalAccuracy, double velocity, double course, bool isLastKnown)
            : base(timestamp, position, horizontalAccuracy, verticalAccuracy, velocity, course, isLastKnown)
        {
        }

        public static NmeaLocation Create(TimeSpan timeOfFix, Rmc rmc, Gga gga, Gsa gsa, Gst gst)
        {
            MapPoint position = null;
            double horizontalAccuracy = double.NaN;
            double verticalAccuracy = double.NaN;
            double velocity = 0;
            double course = 0;
            // Prefer GGA over RMC for location
            if (gga != null && gga.FixTime == timeOfFix)
            {
                if (double.IsNaN(gga.Altitude))
                    position = new MapPoint(gga.Longitude, gga.Latitude, SpatialReferences.Wgs84);
                else
                {
                    // Vertical id 115700 == ellipsoid reference system. Gga is geoid, but we subtract GeoidalSeparation to simplify 
                    // vertical transformations from the simpler/better known ellipsoidal model
                    position = new MapPoint(gga.Longitude, gga.Latitude, gga.Altitude + gga.GeoidalSeparation, SpatialReference.Create(4326, 115700));
                }
            }
            if (rmc != null && rmc.FixTime.TimeOfDay == timeOfFix)
            {
                if (position == null)
                {
                    position = new MapPoint(rmc.Longitude, rmc.Latitude, SpatialReferences.Wgs84);
                }
                velocity = double.IsNaN(rmc.Speed) ? 0 : rmc.Speed;
                course = double.IsNaN(rmc.Course) ? 0 : rmc.Course;
            }
            if (gst != null && gst.FixTime == timeOfFix)
            {
                verticalAccuracy = gst.SigmaHeightError;
                horizontalAccuracy = gst.SemiMajorError;
            }
            if (position == null)
                return null;
            var location = new NmeaLocation(DateTimeOffset.UtcNow.Date.Add(timeOfFix), position, horizontalAccuracy, verticalAccuracy, velocity, course, false);
            location.Rmc = rmc;
            location.Gga = gga;
            location.Gsa = gsa;
            location.Gst = gst?.FixTime == timeOfFix ? gst : null;
            return location;
        }
        public Rmc Rmc { get; private set; }
        public Gga Gga { get; private set; }
        public Gsa Gsa { get; private set; }
        public Gst Gst { get; private set; }

        public int NumberOfSatellites => Gga?.NumberOfSatellites ?? -1;
        public double Hdop => Gsa?.Hdop ?? Gga?.Hdop ?? double.NaN;
        public double Pdop => Gsa?.Pdop ?? double.NaN;
        public double Vdop => Gsa?.Vdop ?? double.NaN;
    }
}
```

![Screenshot](https://user-images.githubusercontent.com/1378165/73328707-95990e80-420f-11ea-85a7-43149e29bd21.png)
