using System;
using System.Collections.Generic;
using System.Globalization;
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
using NmeaParser;
using NmeaParser.Messages;

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

		public Gsv GsvMessage
		{
			get { return (Gsv)GetValue(GsvMessageProperty); }
			set { SetValue(GsvMessageProperty, value); }
		}

		public static readonly DependencyProperty GsvMessageProperty =
			DependencyProperty.Register(nameof(GsvMessage), typeof(Gsv), typeof(SatelliteView), new PropertyMetadata(null, OnGsvMessagePropertyChanged));

		private static void OnGsvMessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var gsv = e.NewValue as Gsv;
			if (gsv == null)
				(d as SatelliteView).satellites.ItemsSource = null;
			else
				(d as SatelliteView).satellites.ItemsSource = gsv.SVs;
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


	public class SatelliteVechicleColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is SatelliteVehicle sv)
			{
				byte alpha = (byte)(sv.SignalToNoiseRatio <= 0 ? 80 : 255);
				switch (sv.TalkerId)
				{
					case Talker.GlobalPositioningSystem: return Color.FromArgb(alpha, 255, 0, 0);
					case Talker.GalileoPositioningSystem: return Color.FromArgb(alpha, 0, 255, 0);
					case Talker.GlonassReceiver: return Color.FromArgb(255, 0, 0, alpha);
					case Talker.GlobalNavigationSatelliteSystem: return Color.FromArgb(alpha, 0, 0, 0);
					default: return Colors.CornflowerBlue;
				}
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}