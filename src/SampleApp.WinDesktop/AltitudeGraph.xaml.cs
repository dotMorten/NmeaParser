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
	/// Interaction logic for AltitudeGraph.xaml
	/// </summary>
	public partial class AltitudeGraph : UserControl
	{
		Queue<double> datapoints = new Queue<double>();
		double min = double.MaxValue;
		double max = double.MinValue;
		public AltitudeGraph()
		{
			InitializeComponent();
			MaxDatapoints = 150;
		}

		public void AddDataPoint(double value)
		{
			if (double.IsNaN(value))
				return;
			datapoints.Enqueue(value);
			min = Math.Min(value, double.IsNaN(min) ? value : min);
			max = Math.Max(value, double.IsNaN(max) ? value : max);
			if (datapoints.Count > MaxDatapoints)
			{
				double val = datapoints.Dequeue();
				//If this is the limiting value, recalculate min/max
				if (val == min)
					min = datapoints.Min();
				if (val == max)
					max = datapoints.Max();
			}
			UpdatePath();
			mintb.Text = min.ToString("0");
			maxtb.Text = max.ToString("0");
		}

		private void UpdatePath()
		{
			if(!datapoints.Any())
			{
				path.Data = null;
				return;
			}
			var data = datapoints.ToArray();
			List<PathSegment> segments = new List<PathSegment>();
			for(int i=1;i<data.Length;i++)
			{
				segments.Add(new LineSegment(new Point(i, max - data[i]), true));
			}
			PathFigure pf = new PathFigure(new Point(0, max - data[0]), segments, false);
			path.Data = new PathGeometry(new PathFigure[] { pf });

		}
		public int MaxDatapoints { get; set; }



		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("Value", typeof(double), typeof(AltitudeGraph), new PropertyMetadata(0d, OnValuePropertyChanged));

		private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((AltitudeGraph)d).AddDataPoint((double)e.NewValue);
		}

		
	}
}
