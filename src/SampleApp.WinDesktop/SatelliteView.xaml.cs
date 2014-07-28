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
	/// Interaction logic for SatelliteView.xaml
	/// </summary>
	public partial class SatelliteView : UserControl
	{
		public SatelliteView()
		{
			InitializeComponent();
		}



		public IEnumerable<NmeaParser.Nmea.Gps.Gpgsv> GpgsvMessages
		{
			get { return (IEnumerable<NmeaParser.Nmea.Gps.Gpgsv>)GetValue(GpgsvMessagesProperty); }
			set { SetValue(GpgsvMessagesProperty, value); }
		}

		// Using a DependencyProperty as the backing store for GpgsvMessages.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty GpgsvMessagesProperty =
			DependencyProperty.Register("GpgsvMessages", typeof(IEnumerable<NmeaParser.Nmea.Gps.Gpgsv>), typeof(SatelliteView), new PropertyMetadata(null, OnGpgsvMessagesChanged));

		private static void OnGpgsvMessagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var sats = e.NewValue as IEnumerable<NmeaParser.Nmea.Gps.Gpgsv>;
			if (sats == null)
				(d as SatelliteView).satellites.ItemsSource = null;
			else
				(d as SatelliteView).satellites.ItemsSource = sats.SelectMany(s => s.SVs);
		}		
	}
	public class PolarPlacementItem : ContentControl
	{
		public PolarPlacementItem()
		{
			HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
			VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
		}
		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			double az = (Azimuth - 90) / 180 * Math.PI;
			double e = (90 - Elevation) / 90;
			double X = Math.Cos(az) * e;
			double Y = Math.Sin(az) * e;
			X = arrangeBounds.Width * .5 * X;
			Y = arrangeBounds.Height * .5 * Y;
			RenderTransform = new TranslateTransform(X, Y);
			return base.ArrangeOverride(arrangeBounds);
		}

		public double Azimuth
		{
			get { return (double)GetValue(AzimuthProperty); }
			set { SetValue(AzimuthProperty, value); }
		}

		public static readonly DependencyProperty AzimuthProperty =
			DependencyProperty.Register("Azimuth", typeof(double), typeof(PolarPlacementItem), new PropertyMetadata(0d, OnAzimuthPropertyChanged));

		private static void OnAzimuthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as PolarPlacementItem).InvalidateArrange();
		}

		public double Elevation
		{
			get { return (double)GetValue(ElevationProperty); }
			set { SetValue(ElevationProperty, value); }
		}

		public static readonly DependencyProperty ElevationProperty =
			DependencyProperty.Register("Elevation", typeof(double), typeof(PolarPlacementItem), new PropertyMetadata(0d, OnElevationPropertyChanged));

		private static void OnElevationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as PolarPlacementItem).InvalidateArrange();
		}

	}
}
