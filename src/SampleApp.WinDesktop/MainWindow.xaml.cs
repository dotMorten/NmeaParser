using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SampleApp.WinDesktop
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			NmeaParser.NmeaDevice device = new NmeaParser.NmeaFileDevice("NmeaSampleData.txt", 50);
			device.MessageReceived += device_MessageReceived;
			var _ = device.OpenAsync();
			mapView.LocationDisplay.LocationProvider = new NmeaLocationProvider(device);
			mapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.Location.AutoPanMode.Navigation;
			mapView.LocationDisplay.IsEnabled = true;
			_ = mapView.ZoomToScaleAsync(5000, TimeSpan.Zero);
		}

		private void device_MessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs args)
		{
			Dispatcher.BeginInvoke((Action) delegate()
			{
				output.Text += args.Message.MessageType + ": " + args.Message.ToString() + '\n';
				output.Select(output.Text.Length - 1, 0); //scroll to bottom

				if(args.Message is NmeaParser.Nmea.Gps.Gpgsv)
				{
					var gpgsv = (NmeaParser.Nmea.Gps.Gpgsv)args.Message;
					if(args.IsMultiPart && args.MessageParts != null)
						satView.GpgsvMessages = args.MessageParts.OfType<NmeaParser.Nmea.Gps.Gpgsv>();
				}
			});
		}
	}
}
