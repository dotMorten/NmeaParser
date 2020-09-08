using Esri.ArcGISRuntime.Mapping;
using NmeaParser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SampleApp.WinDesktop
{
    /// <summary>
    /// Interaction logic for View2D.xaml
    /// </summary>
    public partial class View2D : UserControl
    {
        public View2D()
        {
            InitializeComponent();
            if (mapView.Map == null)
            {
                mapView.Map = new Map(Basemap.CreateNavigationVector());
            }

            mapView.LocationDisplay.InitialZoomScale = 5000;
            mapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.UI.LocationDisplayAutoPanMode.Recenter;
            this.IsVisibleChanged += View2D_IsVisibleChanged;
        }

        private void View2D_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            mapView.LocationDisplay.IsEnabled = IsVisible;
        }

        public NmeaParser.Gnss.GnssMonitor GnssMonitor
        {
            get { return (NmeaParser.Gnss.GnssMonitor)GetValue(GnssMonitorProperty); }
            set { SetValue(GnssMonitorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GnssMonitorProperty =
            DependencyProperty.Register(nameof(GnssMonitor), typeof(NmeaParser.Gnss.GnssMonitor), typeof(View2D), new PropertyMetadata(null, OnGnssMonitorPropertyChanged));

        private static void OnGnssMonitorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((View2D)d).InitNmeaProvider(e.OldValue as NmeaParser.Gnss.GnssMonitor, e.NewValue as NmeaParser.Gnss.GnssMonitor);
        }

        private void InitNmeaProvider(NmeaParser.Gnss.GnssMonitor oldDevice, NmeaParser.Gnss.GnssMonitor newDevice)
        {
            mapView.LocationDisplay.IsEnabled = false;
            if (newDevice != null)
            {
                mapView.LocationDisplay.DataSource = new NmeaLocationDataSource(newDevice, false);
                mapView.LocationDisplay.IsEnabled = IsLoaded;
            }
        }
    }
}
