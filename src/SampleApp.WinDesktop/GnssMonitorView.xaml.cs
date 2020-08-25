using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
            SetupBindings();
        }

        private void SetupBindings()
        {            
            var props = typeof(GnssMonitor).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            List<KeyValuePair<string, object>> values = new List<KeyValuePair<string, object>>();
            int count = 0;
            ArrayConverter conv = new ArrayConverter();
            foreach (var prop in props.OrderBy(t => t.Name))
            {
                if (prop.Name == nameof(GnssMonitor.AllMessages) || prop.Name == nameof(GnssMonitor.SynchronizationContext)) continue;
                data.RowDefinitions.Add(new RowDefinition());
                var title = new TextBlock() { Text = prop.Name, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = VerticalAlignment.Top };
                Grid.SetRow(title, count);
                data.Children.Add(title);
                var valuebox = new TextBlock() { VerticalAlignment = VerticalAlignment.Top, TextWrapping = TextWrapping.Wrap };
                Grid.SetRow(valuebox, count);
                Grid.SetColumn(valuebox, 1);
                var binding = new Binding(prop.Name) { Converter = conv };
                valuebox.SetBinding(TextBlock.TextProperty, binding);
                data.Children.Add(valuebox);
                count++;
            }
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
            data.DataContext = Monitor;
        }

        private class ArrayConverter : IValueConverter
        {
            public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
            {
                if (!(value is string) && value is System.Collections.IEnumerable arr)
                {
                    var str = "[" + string.Join(",", arr.OfType<object>().ToArray()) + "]";
                    if (str.Length == 2)
                        str = "[ ]";
                    value = str;
                }
                if (value is null)
                    return "<null>";
                return value;
            }

            public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
