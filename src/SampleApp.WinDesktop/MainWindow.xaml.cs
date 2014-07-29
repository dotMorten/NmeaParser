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
			_ = mapView.ZoomToScaleAsync(25000);
		}

		Dictionary<int, NmeaParser.Nmea.Gps.Gpgsv> gpgsvList = new Dictionary<int,NmeaParser.Nmea.Gps.Gpgsv>();
		private void device_MessageReceived(NmeaParser.NmeaDevice sender, NmeaParser.Nmea.NmeaMessage args)
		{
			Dispatcher.BeginInvoke((Action) delegate()
			{
				output.Text += args.MessageType + ": " + args.ToString() + '\n';
				output.Select(output.Text.Length - 1, 0); //scroll to bottom

				//Merge all gpgsv satellite messages
				if(args is NmeaParser.Nmea.Gps.Gpgsv)
				{
					var gpgsv = (NmeaParser.Nmea.Gps.Gpgsv)args;
					if(gpgsv.MessageNumber == 1)
					{
						gpgsvList = new Dictionary<int,NmeaParser.Nmea.Gps.Gpgsv>(); //first one. Replace list
					}
					gpgsvList[gpgsv.MessageNumber] = gpgsv;
					if(gpgsv.MessageNumber == gpgsv.TotalMessages)
						satView.GpgsvMessages = gpgsvList.Values;
				}
			});
		}
	}
}
