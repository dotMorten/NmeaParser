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
NMEA often happens in a burst of messages, which could be combined to one larger location object with more information available, as well as containing information from multiple different satellite systems.
By using the `GnssMonitor` class that aggregates these messages, we can create a much more robust location provider:
```
using System;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using NmeaParser.Gnss;

namespace NmeaParser.ArcGIS
{
   public class NmeaLocationDataSource : Esri.ArcGISRuntime.Location.LocationDataSource
    {
        private static SpatialReference wgs84_ellipsoidHeight = SpatialReference.Create(4326, 115700);
        private readonly GnssMonitor m_gnssMonitor;
        private readonly bool m_startStopDevice;
        private double lastCourse = 0; // Course can fallback to NaN, but ArcGIS Datasource don't allow NaN course, so we cache last known as a fallback

        /// <summary>
        /// Initializes a new instance of the <see cref="NmeaLocationDataSource"/> class.
        /// </summary>
        /// <param name="device">The NMEA device to monitor</param>
        /// <param name="startStopDevice">Whether starting this datasource also controls the underlying NMEA device</param>
        public NmeaLocationDataSource(NmeaParser.NmeaDevice device, bool startStopDevice = true) : this(new GnssMonitor(device), startStopDevice)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NmeaLocationDataSource"/> class.
        /// </summary>
        /// <param name="monitor">The NMEA device to monitor</param>
        /// <param name="startStopDevice">Whether starting this datasource also controls the underlying NMEA device</param>
        public NmeaLocationDataSource(NmeaParser.Gnss.GnssMonitor monitor, bool startStopDevice = true)
        {
            if (monitor == null)
                throw new ArgumentNullException(nameof(monitor));
            this.m_gnssMonitor = monitor;
            m_startStopDevice = startStopDevice;
        }

        protected async override Task OnStartAsync()
        {
            m_gnssMonitor.LocationChanged += OnLocationChanged;
            m_gnssMonitor.LocationLost += OnLocationChanged;
            if (m_startStopDevice && !this.m_gnssMonitor.Device.IsOpen)
                await this.m_gnssMonitor.Device.OpenAsync();

            if (m_gnssMonitor.IsFixValid)
                OnLocationChanged(this, EventArgs.Empty);
        }

        protected override Task OnStopAsync()
        {
            m_gnssMonitor.LocationChanged -= OnLocationChanged;
            m_gnssMonitor.LocationLost -= OnLocationChanged;
            if(m_startStopDevice)
                return m_gnssMonitor.Device.CloseAsync();
            else
                return Task.CompletedTask;
        }

        private Esri.ArcGISRuntime.Location.Location currentLocation;

        private void OnLocationChanged(object sender, EventArgs e)
        {
            if (double.IsNaN(m_gnssMonitor.Longitude) || double.IsNaN(m_gnssMonitor.Latitude)) return;
            if (!double.IsNaN(m_gnssMonitor.Course))
                lastCourse = m_gnssMonitor.Course;
            DateTimeOffset? timestamp = null;
            if(m_gnssMonitor.FixTime.HasValue)
                timestamp = new DateTimeOffset(DateTime.UtcNow.Date.Add(m_gnssMonitor.FixTime.Value));
            var location = new Esri.ArcGISRuntime.Location.Location(
                timestamp: timestamp,
                position: !double.IsNaN(m_gnssMonitor.Altitude) ? new MapPoint(m_gnssMonitor.Longitude, m_gnssMonitor.Latitude, m_gnssMonitor.Altitude, wgs84_ellipsoidHeight) : new MapPoint(m_gnssMonitor.Longitude, m_gnssMonitor.Latitude, SpatialReferences.Wgs84),
                horizontalAccuracy: m_gnssMonitor.HorizontalError,
                verticalAccuracy: m_gnssMonitor.VerticalError,
                velocity: double.IsNaN(m_gnssMonitor.Speed) ? 0 : m_gnssMonitor.Speed * 0.51444444,
                course: lastCourse,
                !m_gnssMonitor.IsFixValid);
            // Avoid raising additional location events if nothing changed
            if (currentLocation == null ||
                currentLocation.Position.X != location.Position.X ||
                currentLocation.Position.Y != location.Position.Y ||
                currentLocation.Position.Z != location.Position.Z ||
                currentLocation.Course != location.Course ||
                currentLocation.Velocity != location.Velocity ||
                currentLocation.HorizontalAccuracy != location.HorizontalAccuracy ||
                currentLocation.VerticalAccuracy != location.VerticalAccuracy ||
                currentLocation.IsLastKnown != location.IsLastKnown ||
                timestamp != location.Timestamp)
            {                
                currentLocation = location;
                UpdateLocation(currentLocation);
            }
        }
    }
}
```

![Screenshot](https://user-images.githubusercontent.com/1378165/73328707-95990e80-420f-11ea-85a7-43149e29bd21.png)
