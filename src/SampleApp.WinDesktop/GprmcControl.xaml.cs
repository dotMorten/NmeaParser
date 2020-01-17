using NmeaParser.Nmea;
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

		public Rmc Message
		{
			get { return (Rmc)GetValue(MessageProperty); }
			set { SetValue(MessageProperty, value); }
		}

		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register("Message", typeof(Rmc), typeof(GprmcControl), new PropertyMetadata(null));
	}
}
