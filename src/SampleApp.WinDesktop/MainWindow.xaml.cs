using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private Queue<string> messages = new Queue<string>(101);
        public NmeaParser.NmeaDevice device;

		public MainWindow()
		{
			InitializeComponent();
            DataContext = this;

            InitializeDevice();
		}

        private async void InitializeDevice()
        {
            // Use serial port:
            // comPort = System.IO.Ports.SerialPort.GetPortNames().First();
            // port = new System.IO.Ports.SerialPort(comPort, 4800);
            // device = new NmeaParser.SerialPortDevice(port);

            // Use UDP:
            // device = new NmeaParser.UdpDevice("192.168.10.100", 15000);

            //Use a log file for playing back logged data
            device = new NmeaParser.NmeaFileDevice("NmeaSampleData.txt");
            device.MessageReceived += device_MessageReceived;
            await device.OpenAsync();
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

        private async void UIButton_Click(object sender, RoutedEventArgs e)
        {
            if (device.IsOpen)
                await device.CloseAsync();
            else
                await device.OpenAsync();
            OnPropertyChanged("UIButtonContent");
        }

        public string UIButtonContent
        {
            get { return string.Format("{0} device", !device.IsOpen ? "Start" : "Stop"); }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
