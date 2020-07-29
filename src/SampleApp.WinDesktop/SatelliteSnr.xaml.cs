using NmeaParser;
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
	/// Interaction logic for SatelliteView.xaml
	/// </summary>
	public partial class SatelliteSnr : UserControl
	{
		public SatelliteSnr()
		{
			InitializeComponent();
		}
		Dictionary<Talker, Gsv> messages = new Dictionary<Talker, Gsv>();
		public void SetGsv(Gsv message)
		{
			messages[message.TalkerId] = message;
			UpdateSatellites();
		}
		public void ClearGsv()
		{
			messages.Clear();
			UpdateSatellites();
		}

		private void UpdateSatellites()
		{
			satellites.ItemsSource = messages.Values.SelectMany(g => g.SVs);
		}		
	}
}
