﻿using NmeaParser.Nmea;
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
	/// Interaction logic for GpgsaControl.xaml
	/// </summary>
	public partial class GpgsaControl : UserControl
	{
		public GpgsaControl()
		{
			InitializeComponent();
		}

		public Gsa Message
		{
			get { return (Gsa)GetValue(GpgsaProperty); }
			set { SetValue(GpgsaProperty, value); }
		}

		public static readonly DependencyProperty GpgsaProperty =
			DependencyProperty.Register("Message", typeof(Gsa), typeof(GpgsaControl), new PropertyMetadata(null));	
	}
}
