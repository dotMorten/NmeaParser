using Esri.ArcGISRuntime.Geometry;
using System;

namespace SampleApp.WinDesktop
{
	public class NmeaLocationProvider : Esri.ArcGISRuntime.Location.ILocationProvider
	{
		public event EventHandler<Esri.ArcGISRuntime.Location.LocationInfo> LocationChanged;
		private NmeaParser.NmeaDevice device;
		double m_Accuracy = double.NaN;

		public NmeaLocationProvider(NmeaParser.NmeaDevice device)
		{
			this.device = device;
			device.MessageReceived += device_MessageReceived;
		}

		void device_MessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs e)
		{
			var message = e.Message;
			if (message is NmeaParser.Nmea.Gps.Garmin.Pgrme)
			{
				m_Accuracy = ((NmeaParser.Nmea.Gps.Garmin.Pgrme)message).HorizontalError;
			}
			else if (message is NmeaParser.Nmea.Gps.Gprmc)
			{
				var rmc = (NmeaParser.Nmea.Gps.Gprmc)message;
				if(rmc.Active && LocationChanged != null)
				{
					LocationChanged(this, new Esri.ArcGISRuntime.Location.LocationInfo()
					{
						Course = rmc.Course,
						Speed = rmc.Speed,
						HorizontalAccuracy = m_Accuracy,
						Location = new Esri.ArcGISRuntime.Geometry.MapPoint(rmc.Longitude, rmc.Latitude, SpatialReferences.Wgs84)
					});
				}
			}
		}

		public System.Threading.Tasks.Task StartAsync()
		{
			return this.device.OpenAsync();
		}

		public System.Threading.Tasks.Task StopAsync()
		{
			m_Accuracy = double.NaN;
			return this.device.CloseAsync();
		}
	}
}
