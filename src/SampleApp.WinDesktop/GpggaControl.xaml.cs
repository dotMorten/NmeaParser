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
	/// Interaction logic for GpggaControl.xaml
	/// </summary>
	public partial class GpggaControl : UserControl
	{
		public GpggaControl()
		{
			InitializeComponent();
		}

		public Gpgga Message
		{
			get { return (Gpgga)GetValue(GpggaProperty); }
			set { SetValue(GpggaProperty, value); }
		}

		public static readonly DependencyProperty GpggaProperty =
			DependencyProperty.Register("Message", typeof(Gpgga), typeof(GpggaControl), new PropertyMetadata(null));
	}
}
