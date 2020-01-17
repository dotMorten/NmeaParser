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
	public partial class SatelliteSnr : UserControl
	{
		public SatelliteSnr()
		{
			InitializeComponent();
		}

		public NmeaParser.Nmea.Gsv GsvMessage
		{
			get { return (NmeaParser.Nmea.Gsv)GetValue(GsvMessageProperty); }
			set { SetValue(GsvMessageProperty, value); }
		}

		public static readonly DependencyProperty GsvMessageProperty =
			DependencyProperty.Register(nameof(GsvMessage), typeof(NmeaParser.Nmea.Gsv), typeof(SatelliteSnr), new PropertyMetadata(null, OnGpgsvMessagePropertyChanged));

		private static void OnGpgsvMessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var gsv = e.NewValue as NmeaParser.Nmea.Gsv;
			if (gsv == null)
				(d as SatelliteSnr).satellites.ItemsSource = null;
			else
				(d as SatelliteSnr).satellites.ItemsSource = gsv.SVs;
		}		
	}
}
