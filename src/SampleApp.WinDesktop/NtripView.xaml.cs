using NmeaParser.Gnss.Ntrip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            }
            catch(System.Exception ex)
            {
                MessageBox.Show("Failed to connect: " + ex.Message);
                return;
            }
            sourceList.ItemsSource = sources.OrderBy(s=>s.CountryCode);
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
                        Debugger.Break();
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
}
