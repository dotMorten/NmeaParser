using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleApp
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
			LoadData();
		}
		public async void LoadData()
		{
			var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///NmeaSampleData.txt"));
			var device = new NmeaParser.NmeaFileDevice(file, 50);
			device.MessageReceived += device_MessageReceived;
			
			mapView.LocationDisplay.LocationProvider = new NmeaLocationProvider(device);
			mapView.LocationDisplay.LocationProvider.LocationChanged += LocationProvider_LocationChanged;
			mapView.LocationDisplay.IsEnabled = true;
		}
		private void device_MessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs args)
		{
			var _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				messages.Enqueue(args.Message.MessageType + ": " + args.Message.ToString());
				if (messages.Count > 100) messages.Dequeue(); //Keep message queue at 100
				output.Text = string.Join("\n", messages.ToArray()) + "\n";
				output.Select(output.Text.Length, 0);
				//var index = output.Text.LastIndexOf("\n");
				//if(index > 0)
				//	output.Select(index + 1, 1);
			});
		}

		

		private void LocationProvider_LocationChanged(object sender, Esri.ArcGISRuntime.Location.LocationInfo e)
		{
			var _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				//Zoom in on first location fix
				mapView.LocationDisplay.LocationProvider.LocationChanged -= LocationProvider_LocationChanged;
				mapView.SetView(e.Location, 5000);
				mapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.Location.AutoPanMode.Navigation;
			});
		}
    }
}
