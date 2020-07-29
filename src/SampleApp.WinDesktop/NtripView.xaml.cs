using NmeaParser.Gnss.Ntrip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            client.DataReceived += Client_DataReceived;
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
        Func<Task> stop;
        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            var stream = ((Button)sender).DataContext as NtripStream;
            if (stream == null)
                return;
            if (stop != null)
            {
                try
                {
                    await stop();
                }
                catch { }
            }
            counter = 0;
            client.Connect(stream.Mountpoint);
            client.Disconnected += (s, e) =>
            {
                Debug.WriteLine("NTRIP Stream Disconnected. Retrying...");
                // Try and reconnect after a disconnect
                client.Connect(stream.Mountpoint);
            };
            stop = client.CloseAsync;
            ntripstatus.Text = $"Connected";
        }

        System.Threading.Tasks.Task writingTask;
        object writeLock = new object();
        long counter = 0;
        private async void Client_DataReceived(object sender, byte[] rtcm)
        {
            var device = MainWindow.currentDevice;
            if (device != null && device.CanWrite)
            {
                try
                {
                    //lock (writeLock)
                    {
                        await device.WriteAsync(rtcm, 0, rtcm.Length);
                        counter += rtcm.Length;
                        Dispatcher.Invoke(() =>
                        {
                            ntripstatus.Text = $"Transmitted {counter} bytes";
                        });
                    }
                }
                catch
                {

                }
            }
            //ParseData(rtcm);
        }
        Queue<byte> rtcmData = new Queue<byte>();
        private void ParseData(byte[] rtcm)
        {
            foreach (var b in rtcm)
                rtcmData.Enqueue(b);
            
        }
    }
}
