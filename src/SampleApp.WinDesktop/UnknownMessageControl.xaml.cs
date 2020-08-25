using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NmeaParser.Messages;

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

		public NmeaMessage Message
		{
			get { return (NmeaMessage)GetValue(MessageProperty); }
			set { SetValue(MessageProperty, value); }
		}

		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register("Message", typeof(NmeaMessage), typeof(UnknownMessageControl), new PropertyMetadata(null, OnMessagePropertyChanged));

        private static void OnMessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (UnknownMessageControl)d;

            if (e.NewValue == null || e.NewValue is UnknownMessage && !(e.OldValue is UnknownMessage))
            {
                ctrl.HeaderPanel.Background = new SolidColorBrush(Colors.LightGray);
            }
            else if ((e.OldValue == null || e.OldValue is UnknownMessage) && !(e.NewValue is UnknownMessage))
            {
                ctrl.HeaderPanel.Background = new SolidColorBrush(Colors.CornflowerBlue);
            }
            if (e.NewValue == null)
                ctrl.Values.ItemsSource = null;
            else if (e.NewValue is UnknownMessage unk)
                ctrl.Values.ItemsSource = unk.Values;
            else
            {
                var props = e.NewValue.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).Where(p => !p.CanWrite);
                List<string> values = new List<string>();
                foreach (var prop in props.OrderBy(t => t.Name))
                {
                    if (prop.Name == nameof(NmeaMessage.MessageType) || prop.Name == nameof(NmeaMessage.Checksum) || prop.Name == nameof(NmeaMessage.Timestamp))
                        continue;
                    var value = prop.GetValue(e.NewValue);
                    if (!(value is string) && value is System.Collections.IEnumerable arr)
                    {
                        var str = "[" + string.Join(",", arr.OfType<object>().ToArray()) + "]";
                        if (str.Length == 2)
                            str = "[ ]";
                        value = str;
                    }
                    values.Add($"{prop.Name}: {value}");
                }
                if (e.NewValue is NmeaMessage msg)
                {
                    var age = (System.Diagnostics.Stopwatch.GetTimestamp() * 1000d / System.Diagnostics.Stopwatch.Frequency) - msg.Timestamp;
                    values.Add($"Timestamp: " + DateTime.Now.AddMilliseconds(-age).TimeOfDay.ToString("h\\:mm\\:ss"));
                }
                //;

                ctrl.Values.ItemsSource = values;
            }
        }
    }
}
