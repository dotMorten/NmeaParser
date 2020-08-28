using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using NmeaParser;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Devices.Perception;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SampleApp.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
		public class DeviceInfo
        {
			public Func<Task<NmeaDevice>> CreateMethod { get; set; }
			public string DisplayName { get; set; }
			public override string ToString() => DisplayName;
        }

		private Queue<string> messages = new Queue<string>(101);
		private NmeaParser.NmeaDevice listener;
		private NmeaParser.Gnss.GnssMonitor monitor;
		private ObservableCollection<DeviceInfo> deviceList = new ObservableCollection<DeviceInfo>();

		public MainPage()
		{
			this.InitializeComponent();
			//Load a simulated GPS device that plays back an NMEA log file
			nmeaDevicePicker.ItemsSource = deviceList;
			deviceList.Add(new DeviceInfo()
			{
				DisplayName = "Simulated NMEA Device",
				CreateMethod = async () =>
				{
					var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///NmeaSampleData.txt"));
					return new NmeaParser.NmeaFileDevice(file);
				}
			});
			nmeaDevicePicker.SelectedIndex = 0;
			LoadDevices();
		}
		
		private async void LoadDevices()
		{
			// Find bluetooth devices
			//Ensure the bluetooth capability is enabled by opening package.appxmanifest in a text editor, 
			// and add the following to the <Capabilities> section:
			//    <m2:DeviceCapability Name="bluetooth.rfcomm">
			//      <m2:Device Id="any">
			//        <m2:Function Type="name:serialPort" />
			//      </m2:Device>
			//    </m2:DeviceCapability>

			//Get list of devices
			var btdevices = await NmeaParser.BluetoothDevice.GetBluetoothSerialDevicesAsync();
			foreach(var item in btdevices)
            {
				deviceList.Add(new DeviceInfo()
				{
					DisplayName = $"{item.Device.Name} (Bluetooth)",
					CreateMethod = () =>
					{
						return Task.FromResult<NmeaParser.NmeaDevice>(new NmeaParser.BluetoothDevice(item));
					}
				});
				
            }

			// Find serial devices
			for (int i = 1; i < 10; i++)
			{
				var selector = SerialDevice.GetDeviceSelector("COM" + i.ToString());
				var devices = await DeviceInformation.FindAllAsync(selector);
				if (devices.Any())
				{
					var deviceInfo = devices.First();
					var serialDevice = await SerialDevice.FromIdAsync(deviceInfo.Id);
					if (serialDevice != null)
					{
						deviceList.Add(new DeviceInfo()
						{
							DisplayName = $"Serial Port {i} (COM{i}) @ 9600 baud",
							CreateMethod = () =>
							{
								//Set up serial device according to device specifications:
								serialDevice.BaudRate = 9600;
								serialDevice.DataBits = 8;
								serialDevice.Parity = SerialParity.None;
								return Task.FromResult<NmeaDevice>(new NmeaParser.SerialPortDevice(serialDevice));
							}
						});
					}
				}
			}
		}

		private async void startButton_Click(object sender, RoutedEventArgs e)
		{
			var info = nmeaDevicePicker.SelectedItem as DeviceInfo;
			if (info != null)
			{
				var device = await info.CreateMethod();
				output.Text = string.Empty;
				messages.Clear();
				device.MessageReceived += device_MessageReceived;
				var _ = device.OpenAsync();
				listener = device;
				startButton.IsEnabled = false;
				stopButton.IsEnabled = true;
				monitor = new NmeaParser.Gnss.GnssMonitor(device);
				gnssMonitorView.DataContext = monitor;
			}
		}

		private void stopButton_Click(object sender, RoutedEventArgs e)
		{
			if(listener != null)
            {
				gnssMonitorView.DataContext = null;
				_ = listener.CloseAsync();
				listener.Dispose();
				listener = null;
				startButton.IsEnabled = true;
				stopButton.IsEnabled = false;
			}
		}

		private void device_MessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs args)
		{
			var _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				messages.Enqueue(args.Message.MessageType + ": " + args.Message.ToString());
				if (messages.Count > 100) messages.Dequeue(); //Keep message queue at 100
				output.Text = string.Join("\n", messages.ToArray());
			});
		}
    }
}