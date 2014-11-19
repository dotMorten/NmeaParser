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
        private NmeaParser.Nmea.Gps.Gprmc _last;
		
		public MainWindow()
		{
			InitializeComponent();

			// Use serial port:
			//var comPort = System.IO.Ports.SerialPort.GetPortNames().First();
			//var port = new System.IO.Ports.SerialPort(comPort, 4800);
			//var device = new NmeaParser.SerialPortDevice(port);

			//Use a log file for playing back logged data
			var device = new NmeaParser.NmeaFileDevice("NmeaSampleData.txt");
			device.MessageReceived += device_MessageReceived;
			var _ = device.OpenAsync();
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
                {
                    var gprmc = args.Message as NmeaParser.Nmea.Gps.Gprmc;
                    gprmcView.Message = _last = gprmc;
                }
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
        
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = sender as TabControl;
            if (_last == null || item.SelectedIndex != 2)
                return;
       
            Browser.Navigate(string.Format("http://www.openstreetmap.org/#map=10/{0}/{1}", _last.Latitude, _last.Longitude));
        }
	}
}
