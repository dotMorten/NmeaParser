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
        private NmeaParser.NmeaDevice device; 

		public MainWindow()
		{
			InitializeComponent();
			device = new NmeaParser.NmeaFileDevice("NmeaSampleData.txt");
			device.MessageReceived += device_MessageReceived;
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
					if(args.IsMultiPart && args.MessageParts != null)
						satView.GpgsvMessages = args.MessageParts.OfType<NmeaParser.Nmea.Gps.Gpgsv>();
				}
			});
		}

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (device.IsOpen)
                await device.CloseAsync();
            else
                await device.OpenAsync();
        }
	}
}
