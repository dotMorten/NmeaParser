using NmeaParser.Gnss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SampleApp.WinDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Queue<string> messages = new Queue<string>(101);
        public static NmeaParser.NmeaDevice currentDevice;
        public static GnssMonitor monitor;

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
            var availableSerialPorts = System.IO.Ports.SerialPort.GetPortNames().OrderBy(s => s);
            serialPorts.ItemsSource = availableSerialPorts;
            serialPorts.SelectedIndex = 0;
            // Use serial portName:
            //var comPort = availableSerialPorts.First();
            //var portName = new System.IO.Ports.SerialPort(comPort, 4800);
            //var device = new NmeaParser.SerialPortDevice(portName);

            //Use a log file for playing back logged data
            var device = new NmeaParser.NmeaFileDevice("NmeaSampleData.txt") { EmulatedBaudRate = 9600, BurstRate = TimeSpan.FromSeconds(1d) };
            _ = StartDevice(device);

            LoadBluetoothDevices();

        }
        public class DeviceInfo
        {
            public Func<Task<NmeaParser.NmeaDevice>> CreateMethod { get; set; }
            public string DisplayName { get; set; }
            public override string ToString() => DisplayName;
        }
        private async void LoadBluetoothDevices()
        {
            var deviceList = new List<DeviceInfo>();
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService"))
            {
                var btdevices = await BluetoothDevice.GetBluetoothSerialDevicesAsync();
                foreach (var item in btdevices)
                {
                    deviceList.Add(new DeviceInfo()
                    {
                        DisplayName = $"{item.Device.Name} (Bluetooth)",
                        CreateMethod = () =>
                        {
                            return Task.FromResult<NmeaParser.NmeaDevice>(new BluetoothDevice(item));
                        }
                    });
                }
            }
            bluetoothDevices.ItemsSource = deviceList;
            if (deviceList.Count > 0)
                bluetoothDevices.SelectedIndex = 0;
        }

        /// <summary>
        /// Unloads the current device, and opens the next device
        /// </summary>
        /// <param name="device"></param>
        private async Task StartDevice(NmeaParser.NmeaDevice device)
        {
            //Clean up old device
            if (currentDevice != null)
            {
                currentDevice.MessageReceived -= device_MessageReceived;
                currentDevice.DeviceDisconnected -= device_DeviceDisconnected;
                if (currentDevice.IsOpen)
                    await currentDevice.CloseAsync();
                currentDevice.Dispose();
                if (gnssMonitorView.Monitor != null)
                {
                    gnssMonitorView.Monitor.LocationChanged -= Monitor_LocationChanged;
                    gnssMonitorView.Monitor = monitor = null;
                }
                mapplot.Clear();
            }
            output.Text = "";
            messages.Clear();
            gprmcView.Message = null;
            gpggaView.Message = null;
            gpgsaView.Message = null;
            gpgllView.Message = null;
            pgrmeView.Message = null;
            satView.ClearGsv();
            satSnr.ClearGsv();
            //Start new device
            currentDevice = device;
            foreach (var child in MessagePanel.Children.OfType<UnknownMessageControl>().ToArray())
            {
                MessagePanel.Children.Remove(child);
            }
            currentDevice.MessageReceived += device_MessageReceived;
            currentDevice.DeviceDisconnected += device_DeviceDisconnected;
            if (device is NmeaParser.NmeaFileDevice)
                currentDeviceInfo.Text = string.Format("NmeaFileDevice( file={0} )", ((NmeaParser.NmeaFileDevice)device).FileName);
            else if (device is NmeaParser.SerialPortDevice)
            {
                currentDeviceInfo.Text = string.Format("SerialPortDevice( port={0}, baud={1} )",
                    ((NmeaParser.SerialPortDevice)device).Port.PortName,
                    ((NmeaParser.SerialPortDevice)device).Port.BaudRate);
            }
            else if (device is BluetoothDevice bd)
            {
                currentDeviceInfo.Text = $"Bluetooth {bd.Service.Device.Name}";
            }
            await device.OpenAsync();
            gnssMonitorView.Monitor = monitor = new GnssMonitor(device);
            gnssMonitorView.Monitor.LocationChanged += Monitor_LocationChanged;
            view2d.GnssMonitor = monitor;
            view3d.GnssMonitor = monitor;
        }

        private void device_DeviceDisconnected(object sender, Exception e)
        {
            MessageBox.Show("Device disconnected: " + e.Message);
        }

        private void Monitor_LocationChanged(object sender, EventArgs e)
        {
            var mon = sender as GnssMonitor;
            mapplot.AddLocation(mon.Latitude, mon.Longitude, mon.Altitude, mon.FixQuality, mon.Gst?.SigmaLatitudeError ?? mon.HorizontalError, mon.Gst?.SigmaLongitudeError ?? mon.HorizontalError, mon.Gst?.SigmaHeightError ?? mon.VerticalError);
        }

        private void device_MessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs args)
        {
            Dispatcher.BeginInvoke((Action)delegate ()
           {
               messages.Enqueue(args.Message.ToString());
               if (messages.Count > 100) messages.Dequeue(); //Keep message queue at 100
                output.Text = string.Join("\n", messages.ToArray());
               output.Select(output.Text.Length - 1, 0); //scroll to bottom

                if (args.Message is NmeaParser.Messages.Gsv gpgsv)
               {
                   satView.SetGsv(gpgsv);
                   satSnr.SetGsv(gpgsv);
               }
               else if (args.Message is NmeaParser.Messages.Rmc)
                   gprmcView.Message = args.Message as NmeaParser.Messages.Rmc;
               else if (args.Message is NmeaParser.Messages.Gga)
                   gpggaView.Message = args.Message as NmeaParser.Messages.Gga;
               else if (args.Message is NmeaParser.Messages.Gsa)
                   gpgsaView.Message = args.Message as NmeaParser.Messages.Gsa;
               else if (args.Message is NmeaParser.Messages.Gll)
                   gpgllView.Message = args.Message as NmeaParser.Messages.Gll;
               else if (args.Message is NmeaParser.Messages.Garmin.Pgrme)
                   pgrmeView.Message = args.Message as NmeaParser.Messages.Garmin.Pgrme;
               else
               {
                   var ctrl = MessagePanel.Children.OfType<UnknownMessageControl>().Where(c => c.Message.MessageType == args.Message.MessageType).FirstOrDefault();
                   if (ctrl == null)
                   {
                       ctrl = new UnknownMessageControl()
                       {
                           Style = this.Resources["card"] as Style
                       };
                       MessagePanel.Children.Add(ctrl);
                   }
                   ctrl.Message = args.Message;
               }
           });
        }

        //Browse to nmea file and create device from selected file
        private async void OpenNmeaLogButton_Click(object sender, RoutedEventArgs e)
        {
            var result = nmeaOpenFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var file = nmeaOpenFileDialog.FileName;
                var device = new NmeaParser.NmeaFileDevice(file);
                try
                {
                    await StartDevice(device);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Failed to start device: " + ex.Message);
                }
            }
        }

        //Creates a serial port device from the selected settings
        private async void ConnectToSerialButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var portName = serialPorts.Text as string;
                var baudRate = int.Parse(baudRates.Text);
                var device = new NmeaParser.SerialPortDevice(new System.IO.Ports.SerialPort(portName, baudRate));
                try
                {
                    await StartDevice(device);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Failed to start device: " + ex.Message);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error connecting: " + ex.Message);
            }
        }


        private async void ConnectToBluetoothButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var info = bluetoothDevices.SelectedItem as DeviceInfo;
                if (info != null)
                {
                    var device = await info.CreateMethod();
                    try
                    {
                        await StartDevice(device);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to start device: " + ex.Message);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error connecting: " + ex.Message);
            }
        }
    }


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
