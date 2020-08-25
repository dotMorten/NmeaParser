using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NmeaParser.Gnss;
using NmeaParser.Messages;

namespace SampleApp.WinDesktop
{
    /// <summary>
    /// Interaction logic for GnssMonitorView.xaml
    /// </summary>
    public partial class GnssMonitorView : UserControl
	{
		public GnssMonitorView()
		{
			InitializeComponent();
		}

		public GnssMonitor Monitor
		{
			get { return (GnssMonitor)GetValue(MonitorProperty); }
			set { SetValue(MonitorProperty, value); }
		}

		public static readonly DependencyProperty MonitorProperty =
			DependencyProperty.Register(nameof(Monitor), typeof(GnssMonitor), typeof(GnssMonitorView), new PropertyMetadata(null, (d,e) => ((GnssMonitorView)d).OnMonitorPropertyChanged(e)));

        private void OnMonitorPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is GnssMonitor oldMonitor)
            {
                oldMonitor.LocationChanged -= LocationChanged;
                oldMonitor.LocationLost -= LocationChanged;
            }
            if (e.NewValue is GnssMonitor newMonitor)
            {
                newMonitor.LocationChanged += LocationChanged;
                newMonitor.LocationLost += LocationChanged;
            }
            UpdateValues();
        }

        private void LocationChanged(object sender, System.EventArgs e)
        {
            Dispatcher.Invoke(UpdateValues);
        }
        private void UpdateValues()
        { 
            if (Monitor == null)
                Values.ItemsSource = null;
            else
            {
                var props = Monitor.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                List<KeyValuePair<string, object>> values = new List<KeyValuePair<string, object>>();
                foreach (var prop in props.OrderBy(t => t.Name))
                {
                    if (prop.Name == nameof(GnssMonitor.AllMessages)) continue;
                    if (prop.PropertyType.IsSubclassOf(typeof(NmeaMessage)))
                        continue;
                    var value = prop.GetValue(Monitor);
                    if (!(value is string) && value is System.Collections.IEnumerable arr)
                    {
                        var str = "[" + string.Join(",", arr.OfType<object>().ToArray()) + "]";
                        if (str.Length == 2)
                            str = "[ ]";
                        value = str;
                    }
                    values.Add(new KeyValuePair<string, object>(prop.Name, value));
                }
                Values.ItemsSource = values;
            }
        }        
    }
}
