using System.Collections.ObjectModel;
using NmeaParser;

namespace SampleApp.Maui
{
    public partial class MainPage : ContentPage
    {
        private Queue<string> messages = new Queue<string>(101);
        private NmeaParser.NmeaDevice? listener;
        private NmeaParser.Gnss.GnssMonitor? monitor;
        private ObservableCollection<DeviceInfo> deviceList = new ObservableCollection<DeviceInfo>();

        public MainPage()
        {
            InitializeComponent();
            nmeaDevicePicker.ItemsSource = deviceList;
            deviceList.Add(new SampleApp.Maui.DeviceInfo("Simulated NMEA Device", () => Task.FromResult<NmeaDevice>(new MauiAssetStreamDevice("NmeaSampleData.txt"))));
            nmeaDevicePicker.SelectedIndex = 0;
            LoadDevices();
        }

        private async void LoadDevices()
        {
            foreach(var device in await DeviceHelper.LoadDevices())
            {
                deviceList.Add(device);
            }
        }

        private async void startButton_Click(object sender, EventArgs e)
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
                gnssMonitorView.BindingContext = monitor;
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            if (listener != null)
            {
                gnssMonitorView.BindingContext = null;
                _ = listener.CloseAsync();
                listener.Dispose();
                listener = null;
                startButton.IsEnabled = true;
                stopButton.IsEnabled = false;
            }
        }
        private void device_MessageReceived(object? sender, NmeaParser.NmeaMessageReceivedEventArgs args)
        {
            var _ = Dispatcher.Dispatch(() =>
            {
                messages.Enqueue(args.Message.MessageType + ": " + args.Message.ToString());
                if (messages.Count > 100) messages.Dequeue(); //Keep message queue at 100
                output.Text = string.Join("\n", messages.ToArray());
            });
        }

        private class MauiAssetStreamDevice : BufferedStreamDevice
        {
            string _filename;
            public MauiAssetStreamDevice(string filename) : base()
            {
                _filename = filename;
            }
            protected override Task<Stream> GetStreamAsync() => FileSystem.OpenAppPackageFileAsync(_filename);
        }

    }
}
