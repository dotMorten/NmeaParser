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

![Screenshot](https://user-images.githubusercontent.com/1378165/73328707-95990e80-420f-11ea-85a7-43149e29bd21.png)
