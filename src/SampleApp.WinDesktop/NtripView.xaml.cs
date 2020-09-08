using NmeaParser.Gnss.Ntrip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
    /// Interaction logic for NtripView.xaml
    /// </summary>
    public partial class NtripView : UserControl
    {
        public NtripView()
        {
            InitializeComponent();
        }
        NmeaParser.Gnss.Ntrip.Client client;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            sourceList.ItemsSource = null;
            if (!int.TryParse(port.Text, out int portNumber))
            {
                MessageBox.Show("Invalid port number");
                return;
            }
            client = new NmeaParser.Gnss.Ntrip.Client(host.Text, portNumber, username.Text, password.Password);
            List<NtripStream> sources;
            try
            {
                sources = client.GetSourceTable().OfType<NtripStream>().ToList();
                if(sources.Count == 0)
                {
                    MessageBox.Show($"No NTRIP data streams found at {host.Text}:{portNumber}");
                    return;
                }
            }
            catch(System.Exception ex)
            {
                MessageBox.Show("Failed to connect: " + ex.Message);
                return;
            }
            if (MainWindow.monitor != null && !double.IsNaN(MainWindow.monitor.Latitude) && !double.IsNaN(MainWindow.monitor.Longitude))
            {
                var lat = MainWindow.monitor.Latitude;
                var lon = MainWindow.monitor.Longitude;
                // Order by closest source
                sourceList.ItemsSource = sources.OrderBy(s => PointPlotView.Vincenty.GetDistanceVincenty(lat, lon, s.Latitude, s.Longitude));
            }
            else
            {
                sourceList.ItemsSource = sources.OrderBy(s => s.CountryCode);
            }
        }
        Stream ntripStream;
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            var streaminfo = ((Button)sender).DataContext as NtripStream;
            if (streaminfo == null)
                return;
            ntripStream?.Dispose();
            var stream = ntripStream = client.OpenStream(streaminfo.Mountpoint);
            _ = Task.Run(async () =>
            {
                byte[] buffer = new byte[1024];
                int count = 0;
                while (true)
                {
                    try
                    {
                        count = await stream.ReadAsync(buffer).ConfigureAwait(false);
                    }
                    catch (System.Exception ex)
                    {
                        if(!stream.CanRead)
                        {
                            // TODO: Restart stream
                            return;
                        }
                    }
                    var device = MainWindow.currentDevice;
                    if (device != null && device.CanWrite)
                    {
                        await device.WriteAsync(buffer, 0, count).ConfigureAwait(false);
                        Dispatcher.Invoke(() =>
                        {
                            ntripstatus.Text = $"Transmitted {ntripStream.Position} bytes";
                        });
                    }
                }
            });
            ntripstatus.Text = $"Connected";
        }
    }
    public class DistanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is NtripStream s && MainWindow.monitor?.IsFixValid == true)
            {
                var lat = MainWindow.monitor.Latitude;
                var lon = MainWindow.monitor.Longitude;
                var distance = PointPlotView.Vincenty.GetDistanceVincenty(lat, lon, s.Latitude, s.Longitude);
                return "Distance: " + (distance / 1000).ToString("0.##") + "km";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
