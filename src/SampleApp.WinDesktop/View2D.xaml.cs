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
        }

        public NmeaDevice NmeaDevice
        {
            get { return (NmeaDevice)GetValue(NmeaDeviceProperty); }
            set { SetValue(NmeaDeviceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NmeaDeviceProperty =
            DependencyProperty.Register(nameof(NmeaDevice), typeof(NmeaDevice), typeof(View2D), new PropertyMetadata(null, OnNmeaDevicePropertyChanged));

        private static void OnNmeaDevicePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((View2D)d).InitNmeaProvider(e.OldValue as NmeaDevice, e.NewValue as NmeaDevice);
        }

        private void InitNmeaProvider(NmeaDevice oldDevice, NmeaDevice newDevice)
        {
            mapView.LocationDisplay.IsEnabled = false;
            if (newDevice != null)
            {
                mapView.LocationDisplay.DataSource = new NmeaLocationDataSource(newDevice);
                mapView.LocationDisplay.IsEnabled = true;
            }
        }
    }
}
