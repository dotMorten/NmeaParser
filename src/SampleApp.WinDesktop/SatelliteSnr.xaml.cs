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

namespace SampleApp
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



		public IEnumerable<NmeaParser.Nmea.Gps.Gpgsv> GpgsvMessages
		{
			get { return (IEnumerable<NmeaParser.Nmea.Gps.Gpgsv>)GetValue(GpgsvMessagesProperty); }
			set { SetValue(GpgsvMessagesProperty, value); }
		}

		// Using a DependencyProperty as the backing store for GpgsvMessages.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty GpgsvMessagesProperty =
			DependencyProperty.Register("GpgsvMessages", typeof(IEnumerable<NmeaParser.Nmea.Gps.Gpgsv>), typeof(SatelliteSnr), new PropertyMetadata(null, OnGpgsvMessagesChanged));

		private static void OnGpgsvMessagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var sats = e.NewValue as IEnumerable<NmeaParser.Nmea.Gps.Gpgsv>;
			if (sats == null)
				(d as SatelliteSnr).satellites.ItemsSource = null;
			else
				(d as SatelliteSnr).satellites.ItemsSource = sats.SelectMany(s => s.SVs);
		}		
	}
}
