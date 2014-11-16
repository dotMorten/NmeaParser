using Esri.ArcGISRuntime.Geometry;
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
		private Queue<string> messages = new Queue<string>(101);
		
		public MainWindow()
		{
			InitializeComponent();
			// Use serial port:
			//var comPort = System.IO.Ports.SerialPort.GetPortNames().First();
			//var port = new System.IO.Ports.SerialPort(comPort, 4800);
			//var device = new NmeaParser.SerialPortDevice(port);

			//Use a log file for playing back logged data
			NmeaParser.NmeaDevice device = new NmeaParser.NmeaFileDevice("NmeaSampleData.txt", 50);
			device.MessageReceived += device_MessageReceived;
			mapView.LocationDisplay.LocationProvider = new NmeaLocationProvider(device);
			mapView.LocationDisplay.IsEnabled = true;
			mapView.LocationDisplay.LocationProvider.LocationChanged += LocationProvider_LocationChanged;
		}

		private void LocationProvider_LocationChanged(object sender, Esri.ArcGISRuntime.Location.LocationInfo e)
		{
			Dispatcher.BeginInvoke((Action)delegate()
			{
				//Zoom in on first location fix
				mapView.LocationDisplay.LocationProvider.LocationChanged -= LocationProvider_LocationChanged;
				mapView.SetView(e.Location, 5000);
				mapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.Location.AutoPanMode.Navigation;
			});
		}
		
		private void device_MessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs args)
		{
			Dispatcher.BeginInvoke((Action) delegate()
			{
				messages.Enqueue(args.Message.MessageType + ": " + args.Message.ToString());
				if (messages.Count > 100) messages.Dequeue(); //Keep message queue at 100
				output.Text = string.Join("\n", messages.ToArray());
				output.Select(output.Text.Length - 1, 0); //scroll to bottom

				if(args.Message is NmeaParser.Nmea.Gps.Gpgsv)
				{
					var gpgsv = (NmeaParser.Nmea.Gps.Gpgsv)args.Message;
					if(args.IsMultipart && args.MessageParts != null)
						satView.GpgsvMessages = args.MessageParts.OfType<NmeaParser.Nmea.Gps.Gpgsv>();
				}
				if (args.Message is NmeaParser.Nmea.Gps.Gprmc)
					gprmcView.Message = args.Message as NmeaParser.Nmea.Gps.Gprmc;
				else if (args.Message is NmeaParser.Nmea.Gps.Gpgga)
					gpggaView.Message = args.Message as NmeaParser.Nmea.Gps.Gpgga;
				else if (args.Message is NmeaParser.Nmea.Gps.Gpgsa)
					gpgsaView.Message = args.Message as NmeaParser.Nmea.Gps.Gpgsa;
				else if (args.Message is NmeaParser.Nmea.Gps.Gpgll)
					gpgllView.Message = args.Message as NmeaParser.Nmea.Gps.Gpgll;
				else if (args.Message is NmeaParser.Nmea.Gps.Garmin.Pgrme)
					pgrmeView.Message = args.Message as NmeaParser.Nmea.Gps.Garmin.Pgrme;
			});
		}
	}
}
