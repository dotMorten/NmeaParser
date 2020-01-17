using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Location;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
#if NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows.Threading;
#endif


namespace SampleApp.WinDesktop
{
	public class RestoreAutoPanMode
	{
		private class DelayTimer
		{
			private Action m_action;
			DispatcherTimer m_timer;
			public DelayTimer(Action action)
			{
				m_timer = new DispatcherTimer();
				m_timer.Tick += m_timer_Tick;
				m_action = action;
			}


#if NETFX_CORE
			void m_timer_Tick(object sender, object e)
#else
			void m_timer_Tick(object sender, EventArgs e)
#endif
			{
				m_timer.Stop();
				if (m_action != null)
					m_action();				
			}
			public void Invoke(TimeSpan delay)
			{
				m_timer.Stop();
				m_timer.Interval = delay;
				m_timer.Start();
			}
			public void Cancel()
			{
				m_timer.Stop();
			}
		}


		private MapView m_mapView;
		private DelayTimer m_timer;


		public RestoreAutoPanMode()
		{
			m_timer = new DelayTimer(ResetPanMode);
			RestoreScale = double.NaN;
		}


		private void ResetPanMode()
		{
			if (m_mapView != null && m_mapView.LocationDisplay != null)
			{
				if (!double.IsNaN(RestoreScale))
					m_mapView.SetViewpointScaleAsync(RestoreScale);
				m_mapView.LocationDisplay.AutoPanMode = this.PanMode;
			}
		}


		internal void AttachToMapView(MapView mv)
		{
			if (m_mapView != null && m_mapView != mv)
				throw new InvalidOperationException("RestoreAutoPanMode can only be assigned to one mapview");
			m_mapView = mv;
			(m_mapView as INotifyPropertyChanged).PropertyChanged += m_mapView_PropertyChanged;
		}

		internal void DetachFromMapView(MapView mv)
		{
            (m_mapView as INotifyPropertyChanged).PropertyChanged -= m_mapView_PropertyChanged;
			m_mapView = null;
		}

		private void m_mapView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			//If user stopped navigating and we're not in the correct autopan mode,
			//restore autopan after the set delay.
			if(IsEnabled && e.PropertyName == "IsNavigating")
			{
				if(m_mapView.LocationDisplay != null && 
					m_mapView.LocationDisplay.AutoPanMode != PanMode)
				{
					if (!m_mapView.IsNavigating)
						m_timer.Invoke(TimeSpan.FromSeconds(DelayInSeconds));
					else
						m_timer.Cancel();
				}
			}
		}

		public bool IsEnabled { get; set; }

		public double DelayInSeconds { get; set; }

		public Esri.ArcGISRuntime.UI.LocationDisplayAutoPanMode PanMode { get; set; }

		public double RestoreScale { get; set; }


		public static RestoreAutoPanMode GetRestoreAutoPanSettings(DependencyObject obj)
		{
			return (RestoreAutoPanMode)obj.GetValue(RestoreAutoPanSettingsProperty);
		}


		public static void SetRestoreAutoPanSettings(DependencyObject obj, RestoreAutoPanMode value)
		{
			obj.SetValue(RestoreAutoPanSettingsProperty, value);
		}


		public static readonly DependencyProperty RestoreAutoPanSettingsProperty =
			DependencyProperty.RegisterAttached("RestoreAutoPanSettings", typeof(RestoreAutoPanMode), typeof(RestoreAutoPanMode), 
			new PropertyMetadata(null, OnRestoreAutoPanSettingsChanged));


		private static void OnRestoreAutoPanSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MapView))
				throw new InvalidOperationException("This property must be attached to a mapview");


			MapView mv = (MapView)d;
			var oldValue = e.OldValue as RestoreAutoPanMode;
			if (oldValue != null)
				oldValue.DetachFromMapView(mv);
			var newValue = e.NewValue as RestoreAutoPanMode;
			if (newValue != null)
				newValue.AttachToMapView(mv);
		}		
	}
}
