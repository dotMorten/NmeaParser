using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
		public MainPage()
        {
            this.InitializeComponent();
			autoPanModeSelector.ItemsSource = Enum.GetValues(typeof(Esri.ArcGISRuntime.Location.AutoPanMode));
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
