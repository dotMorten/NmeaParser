using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml;

namespace SampleApp
{
	public class Device : ModelBase
	{
		public Device(DeviceInformation deviceInfo)
		{
			DeviceInfo = deviceInfo;
		}
		public DeviceInformation DeviceInfo { get; private set; }

		private bool m_IsConnnected;

		public bool IsConnected
		{
			get { return m_IsConnnected; }
			set { m_IsConnnected = value; OnPropertyChanged(); }
		}
		
		public NmeaParser.NmeaDevice NmeaInstance { get; set; }
		public override string ToString()
		{
			if (DeviceInfo != null)
				return DeviceInfo.Name;
			else 
				return "Simulator";
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
	public class MainPageVM : ModelBase
	{
		private Windows.UI.Core.CoreDispatcher Dispatcher;
		public MainPageVM()
		{
			Dispatcher = SampleApp.Store.App.Current.Resources.Dispatcher;
			Messages = new ObservableCollection<string>();
			BluetoothDevices = new ObservableCollection<Device>();
			m_LocationProvider = new NmeaLocationProvider(null);
			LocationDisplay = new Esri.ArcGISRuntime.Location.LocationDisplay() { LocationProvider = m_LocationProvider, AutoPanMode = Esri.ArcGISRuntime.Location.AutoPanMode.Navigation, IsEnabled = true };
			m_ConnectCommand = new DelegateCommand(
				(device) => ToggleDeviceConnection(device as Device),
				(d) => { return (d is Device); });
			Overlays = new Esri.ArcGISRuntime.Controls.GraphicsOverlayCollection();
			Overlays.Add(new Esri.ArcGISRuntime.Controls.GraphicsOverlay()
			{
				Renderer = new SimpleRenderer()
				{
					Symbol = new SimpleMarkerSymbol()
					{
						Color = Windows.UI.Colors.CornflowerBlue, Size = 14
					}
				}
			});
			LoadDevices();
		}

		private async void LoadDevices()
		{
			//Ensure the bluetooth capability is enabled by opening package.appxmanifest in a text editor, 
			// and add the following to the <Capabilities> section:
			//    <m2:DeviceCapability Name="bluetooth.rfcomm">
			//      <m2:DeviceInfo Id="any">
			//        <m2:Function Type="name:serialPort" />
			//      </m2:DeviceInfo>
			//    </m2:DeviceCapability>

			string serialDeviceType = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
			var devices = await DeviceInformation.FindAllAsync(serialDeviceType);
			foreach (var deviceInfo in devices)
				BluetoothDevices.Add(new Device(deviceInfo));
			//Add simulation device
			BluetoothDevices.Add(new Device(null));
		}

		public async Task<NmeaParser.NmeaDevice> LoadLocalGpsSimulationData()
		{
			var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///NmeaSampleData.txt"));
			var device = new NmeaParser.NmeaFileDevice(file);
			return device;
		}

		private void ToggleDeviceConnection(Device device)
		{
			if(device.IsConnected)
				CloseDevice(device);
			else
				OpenDevice(device);
		}

		private async void OpenDevice(Device device)
		{
			var bluetoothDevice = device.DeviceInfo;
			NmeaParser.NmeaDevice nmeadevice = null;
			if (bluetoothDevice == null)
			{
				nmeadevice = await LoadLocalGpsSimulationData();
			}
			else
			{
				RfcommDeviceService rfcommService = await RfcommDeviceService.FromIdAsync(bluetoothDevice.Id);
				if (rfcommService == null)
					return;
				nmeadevice = new NmeaParser.BluetoothDevice(rfcommService);
			}
			nmeadevice.MessageReceived += device_MessageReceived;
			device.NmeaInstance = nmeadevice;
			device.IsConnected = true;
			await nmeadevice.OpenAsync();
		}
		private async void CloseDevice(Device device)
		{
			device.IsConnected = false;
			device.NmeaInstance.MessageReceived -= device_MessageReceived;
			await device.NmeaInstance.CloseAsync();
			device.NmeaInstance = null;
		}

		private void LocationProvider_LocationChanged(object sender, Esri.ArcGISRuntime.Location.LocationInfo e)
		{
			var _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				//TODO: Zoom
			});
		}
		private void device_MessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs args)
		{
			var _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{

				Messages.Add(args.Message.MessageType + ": " + args.Message.ToString());
				if (Messages.Count > 100) Messages.RemoveAt(0); //Keep message queue at 100
				m_LocationProvider.ParseMessage(args.Message);

				if(args.Message is NmeaParser.Nmea.LaserRange.LaserRangeMessage)
				{
					RangeMessage = (NmeaParser.Nmea.LaserRange.LaserRangeMessage)args.Message;
					OnPropertyChanged("RangeMessage");
					AddLaserRangePoint(RangeMessage);
				}
			});
		}

		private void AddLaserRangePoint(NmeaParser.Nmea.LaserRange.LaserRangeMessage rangeMessage)
		{
			if (LocationDisplay.CurrentLocation == null || LocationDisplay.CurrentLocation.Location == null) 
				return;
			var pointShot = GeometryEngine.GeodesicMove(LocationDisplay.CurrentLocation.Location,
				rangeMessage.HorizontalDistance, LinearUnits.Meters,
				rangeMessage.HorizontalAngle);
			GraphicsOverlay overlay = Overlays[0];
			overlay.Graphics.Add(new Graphic(pointShot));
		}

		public Esri.ArcGISRuntime.Controls.GraphicsOverlayCollection Overlays { get; private set; }

		public ObservableCollection<string> Messages { get; private set; }

		public ObservableCollection<Device> BluetoothDevices
		{
			get;
			private set;
		}

		private NmeaLocationProvider m_LocationProvider;

		public Esri.ArcGISRuntime.Location.ILocationProvider LocationProvider
		{
			get
			{
				return m_LocationProvider;
			}
		}

		public Esri.ArcGISRuntime.Location.LocationDisplay LocationDisplay
		{
			get;
			private set;
		}

		private DelegateCommand m_ConnectCommand;
		public System.Windows.Input.ICommand ConnectCommand
		{
			get { return m_ConnectCommand; }
		}

		public NmeaParser.Nmea.LaserRange.LaserRangeMessage RangeMessage { get; private set; }
	}

	internal class DelegateCommand : System.Windows.Input.ICommand
	{
		private Action<object> m_executeAction;
		private Func<object, bool> m_canExecuteAction;
		public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecuteAction = null)
		{
			m_executeAction = executeAction;
			m_canExecuteAction = canExecuteAction;
		}
		public bool CanExecute(object parameter)
		{
			return m_canExecuteAction == null || m_canExecuteAction(parameter);
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			m_executeAction(parameter);
		}
		public void RaiseCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
				CanExecuteChanged(this, EventArgs.Empty);
		}
	}

	public class ReverseConverter : Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return -(double)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return -(double)value;
		}
	}

	public class NullToCollapsedConverter : Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return value == null ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return -(double)value;
		}
	}
}
