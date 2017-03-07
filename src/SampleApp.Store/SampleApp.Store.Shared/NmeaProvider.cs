using Esri.ArcGISRuntime.Geometry;
using System;
using System.Threading.Tasks;

namespace SampleApp
{
	public class NmeaLocationProvider : Esri.ArcGISRuntime.Location.LocationDataSource
	{
		private NmeaParser.NmeaDevice device;
		double m_Accuracy = double.NaN;
		double m_altitude = double.NaN;

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
			if (message is NmeaParser.Nmea.Gps.Garmin.Pgrme)
			{
				m_Accuracy = ((NmeaParser.Nmea.Gps.Garmin.Pgrme)message).HorizontalError;
			}
			else if (message is NmeaParser.Nmea.Gps.Gpgga)
			{
				m_altitude = ((NmeaParser.Nmea.Gps.Gpgga)message).Altitude;
			}
			else if (message is NmeaParser.Nmea.Gps.Gprmc)
			{
				var rmc = (NmeaParser.Nmea.Gps.Gprmc)message;
				if(rmc.Active)
				{
                    base.UpdateLocation(new Esri.ArcGISRuntime.Location.Location(
                        new MapPoint(rmc.Longitude, rmc.Latitude, m_altitude, SpatialReferences.Wgs84),
                        m_Accuracy, rmc.Speed, rmc.Course, false));
				}
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
	}
}
