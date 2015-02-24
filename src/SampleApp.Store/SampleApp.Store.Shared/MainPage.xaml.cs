using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleApp.Store
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
