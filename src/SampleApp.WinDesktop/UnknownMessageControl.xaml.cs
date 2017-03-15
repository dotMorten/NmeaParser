using NmeaParser.Nmea;
using NmeaParser.Nmea.Gps;
using NmeaParser.Nmea.Gps.Garmin;
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
    /// Interaction logic for UnknownMessageControl.xaml
    /// </summary>
    public partial class UnknownMessageControl : UserControl
	{
		public UnknownMessageControl()
		{
			InitializeComponent();
		}

		public UnknownMessage Message
		{
			get { return (UnknownMessage)GetValue(MessageProperty); }
			set { SetValue(MessageProperty, value); }
		}

		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register("Message", typeof(NmeaParser.Nmea.UnknownMessage), typeof(UnknownMessageControl), new PropertyMetadata(null));
	}
}
