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
		private NmeaParser.NmeaDevice currentDevice;
		//Dialog for browsing to nmea log files
		private Microsoft.Win32.OpenFileDialog nmeaOpenFileDialog = new Microsoft.Win32.OpenFileDialog()
		{
			Filter = "Text files|*.txt|NMEA Log|*.nmea|All files|*.*",
			InitialDirectory = new System.IO.FileInfo(typeof(MainWindow).Assembly.Location).DirectoryName
		};
		public MainWindow()
		{
			InitializeComponent();

			//Get list of serial ports for device tab
			var availableSerialPorts = System.IO.Ports.SerialPort.GetPortNames().OrderBy(s=>s);
			serialPorts.ItemsSource = availableSerialPorts;
			serialPorts.SelectedIndex = 0;
			// Use serial portName:
			//var comPort = availableSerialPorts.First();
			//var portName = new System.IO.Ports.SerialPort(comPort, 4800);
			//var device = new NmeaParser.SerialPortDevice(portName);

			//Use a log file for playing back logged data
			var device = new NmeaParser.NmeaFileDevice("NmeaSampleData.txt");

			StartDevice(device);
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

		/// <summary>
		/// Unloads the current device, and opens the next device
		/// </summary>
		/// <param name="device"></param>
		private void StartDevice(NmeaParser.NmeaDevice device)
		{
			//Clean up old device
			if (currentDevice != null)
			{
				currentDevice.MessageReceived -= device_MessageReceived;
				currentDevice.Dispose();
				mapView.LocationDisplay.LocationProvider.LocationChanged -= LocationProvider_LocationChanged;
				mapView.LocationDisplay.LocationProvider = null;
			}
			output.Text = "";
			messages.Clear();
			gprmcView.Message = null;
			gpggaView.Message = null;
			gpgsaView.Message = null;
			gpgllView.Message = null;
			pgrmeView.Message = null;
			satView.GpgsvMessages = null;
			//Start new device
			currentDevice = device;
			currentDevice.MessageReceived += device_MessageReceived;
			mapView.LocationDisplay.LocationProvider = new NmeaLocationProvider(device);
			mapView.LocationDisplay.IsEnabled = true;
			mapView.LocationDisplay.LocationProvider.LocationChanged += LocationProvider_LocationChanged;

			//var _ = currentDevice.OpenAsync();
			if (device is NmeaParser.NmeaFileDevice)
				currentDeviceInfo.Text = string.Format("NmeaFileDevice( file={0} )", ((NmeaParser.NmeaFileDevice)device).FileName);
			else if (device is NmeaParser.SerialPortDevice)
			{
				currentDeviceInfo.Text = string.Format("SerialPortDevice( port={0}, baud={1} )",
					((NmeaParser.SerialPortDevice)device).Port.PortName,
					((NmeaParser.SerialPortDevice)device).Port.BaudRate);
			}
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

		//Browse to nmea file and create device from selected file
		private void OpenNmeaLogButton_Click(object sender, RoutedEventArgs e)
		{
			var result = nmeaOpenFileDialog.ShowDialog();
			if (result.HasValue && result.Value)
			{
				var file = nmeaOpenFileDialog.FileName;
				var device = new NmeaParser.NmeaFileDevice(file, 100);
				StartDevice(device);
			}
		}

		//Creates a serial port device from the selected settings
		private void ConnectToSerialButton_Click(object sender, RoutedEventArgs e)
		{
			var portName = serialPorts.Text as string;
			var baudRate = int.Parse(baudRates.Text);
			var device = new NmeaParser.SerialPortDevice(new System.IO.Ports.SerialPort(portName, baudRate));
			StartDevice(device);
		}

		//Attempts to perform an auto discovery of serial ports
		private async void AutoDiscoverButton_Click(object sender, RoutedEventArgs e)
		{
			var button = sender as Button;
			button.IsEnabled = false;
			System.IO.Ports.SerialPort port = await Task.Run<System.IO.Ports.SerialPort>(() => {
				return FindPort(
					new System.Progress<string>((s) => { Dispatcher.BeginInvoke((Action)delegate() { autoDiscoverStatus.Text = s; }); }));
			});
			if (port != null) //we found a port
			{
				autoDiscoverStatus.Text = "";
				serialPorts.Text = port.PortName;
				baudRates.Text = port.BaudRate.ToString();
				ConnectToSerialButton_Click(sender, e);
			}
			else
				autoDiscoverStatus.Text = "No GPS port found";
			button.IsEnabled = false;
		}

		//Iterates all serial ports and attempts to open them at different baud rates
		//and looks for a GPS message.
		private static System.IO.Ports.SerialPort FindPort(IProgress<string> progress = null)
		{
			var ports = System.IO.Ports.SerialPort.GetPortNames().OrderBy(s => s);
			foreach (var portName in ports)
			{
				using (var port = new System.IO.Ports.SerialPort(portName))
				{
					var defaultRate = port.BaudRate;
					List<int> baudRatesToTest = new List<int>(new[] { 9600, 4800, 115200, 19200, 57600, 38400, 2400 }); //Ordered by likelihood
					//Move default rate to first spot
					if (baudRatesToTest.Contains(defaultRate)) baudRatesToTest.Remove(defaultRate);
					baudRatesToTest.Insert(0, defaultRate);
					foreach (var baud in baudRatesToTest)
					{

						if (progress != null)
							progress.Report(string.Format("Trying {0} @ {1}baud", portName, port.BaudRate));
						port.BaudRate = baud;
						port.ReadTimeout = 2000; //this might not be long enough
						bool success = false;
						try
						{
							port.Open();
							if (!port.IsOpen)
								continue; //couldn't open port
							try
							{
								port.ReadTo("$GP");
							}
							catch (TimeoutException)
							{
								continue;
							}
						}
						catch
						{
							//Error reading
						}
						finally
						{
							port.Close();
						}
						if (success)
						{
							return new System.IO.Ports.SerialPort(portName, baud);
						}
					}
				}
			}
			return null;
		}
	}
}
namespace SampleApp
{
	public class ReverseConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return -(double)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return -(double)value;
		}
	}
}
