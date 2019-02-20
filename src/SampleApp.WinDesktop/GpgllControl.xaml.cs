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
	/// Interaction logic for GpgllControl.xaml
	/// </summary>
	public partial class GpgllControl : UserControl
	{
		public GpgllControl()
		{
			InitializeComponent();
		}

		public Gll Message
		{
			get { return (Gll)GetValue(MessageProperty); }
			set { SetValue(MessageProperty, value); }
		}

		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register("Message", typeof(Gll), typeof(GpgllControl), new PropertyMetadata(null));	
	}
}
