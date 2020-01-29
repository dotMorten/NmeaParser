using NmeaParser.Messages;
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
	/// Interaction logic for GgaControl.xaml
	/// </summary>
	public partial class GgaControl : UserControl
	{
		public GgaControl()
		{
			InitializeComponent();
		}

		public Gga Message
		{
			get { return (Gga)GetValue(MessageProperty); }
			set { SetValue(MessageProperty, value); }
		}

		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register(nameof(Message), typeof(Gga), typeof(GgaControl), new PropertyMetadata(null));
	}
}
