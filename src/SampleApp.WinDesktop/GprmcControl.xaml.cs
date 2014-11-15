using NmeaParser.Nmea.Gps;
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
	/// Interaction logic for GprmcControl.xaml
	/// </summary>
	public partial class GprmcControl : UserControl
	{
		public GprmcControl()
		{
			InitializeComponent();
		}

		public Gprmc Message
		{
			get { return (Gprmc)GetValue(MessageProperty); }
			set { SetValue(MessageProperty, value); }
		}

		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register("Message", typeof(Gprmc), typeof(GprmcControl), new PropertyMetadata(null));
	}
}
