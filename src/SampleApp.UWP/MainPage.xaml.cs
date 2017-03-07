using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
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
        private Queue<string> messages = new Queue<string>(101);

		public MainPage()
		{
			this.InitializeComponent();
			//Load a simulated GPS device that plays back an NMEA log file
			LoadLocalGpsSimulationData();
			//Use this to connect to a bluetooth device instead:
			//LoadBluetoothGPS();
			//Use this to connect to a serial port device instead:
			//LoadSerialDeviceGPS();
		}

		private async void LoadSerialDeviceGPS()
		{
			var selector = SerialDevice.GetDeviceSelector("COM3");
			var devices = await DeviceInformation.FindAllAsync(selector);
			if(devices.Any())
			{
				var deviceInfo = devices.First();
				var serialDevice = await SerialDevice.FromIdAsync(deviceInfo.Id);
				//Set up serial device according to device specifications:
				serialDevice.BaudRate = 4800;
				serialDevice.DataBits = 8;
				serialDevice.Parity = SerialParity.None;
				var device = new NmeaParser.SerialPortDevice(serialDevice);
				device.MessageReceived += device_MessageReceived;
				await device.OpenAsync();
			}
		}

		private async void LoadBluetoothGPS()
		{
			//Ensure the bluetooth capability is enabled by opening package.appxmanifest in a text editor, 
			// and add the following to the <Capabilities> section:
			//    <m2:DeviceCapability Name="bluetooth.rfcomm">
			//      <m2:Device Id="any">
			//        <m2:Function Type="name:serialPort" />
			//      </m2:Device>
			//    </m2:DeviceCapability>

			//Get list of devices
			string serialDeviceType = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
			var devices = await DeviceInformation.FindAllAsync(serialDeviceType);

			string GpsDeviceName = "HOLUX GPSlim236"; //Change name to your device or build UI for user to select from list of 'devices'
													  //Select device by name
			var bluetoothDevice = devices.Where(t => t.Name == GpsDeviceName).FirstOrDefault();
			//Get service
			RfcommDeviceService rfcommService = await RfcommDeviceService.FromIdAsync(bluetoothDevice.Id);
			if (rfcommService != null)
			{
				var device = new NmeaParser.BluetoothDevice(rfcommService);
				device.MessageReceived += device_MessageReceived;
				await device.OpenAsync();
			}
		}

		public async void LoadLocalGpsSimulationData()
		{
			var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///NmeaSampleData.txt"));
			var device = new NmeaParser.NmeaFileDevice(file);
			device.MessageReceived += device_MessageReceived;
			var _ = device.OpenAsync();
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