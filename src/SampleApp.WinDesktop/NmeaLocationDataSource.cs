//  *******************************************************************************
//  *  Licensed under the Apache License, Version 2.0 (the "License");
//  *  you may not use this file except in compliance with the License.
//  *  You may obtain a copy of the License at
//  *
//  *  http://www.apache.org/licenses/LICENSE-2.0
//  *
//  *   Unless required by applicable law or agreed to in writing, software
//  *   distributed under the License is distributed on an "AS IS" BASIS,
//  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *   See the License for the specific language governing permissions and
//  *   limitations under the License.
//  ******************************************************************************

using System;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using NmeaParser.Gnss;

namespace SampleApp.WinDesktop
{
    public class NmeaLocationDataSource : Esri.ArcGISRuntime.Location.LocationDataSource
    {
        private static SpatialReference wgs84_ellipsoidHeight = SpatialReference.Create(4326, 115700);
        private readonly GnssMonitor m_gnssMonitor;
        private readonly bool m_startStopDevice;
        private double lastCourse = 0; // Course can fallback to NaN, but ArcGIS Datasource don't allow NaN course, so we cache last known as a fallback

        /// <summary>
        /// Initializes a new instance of the <see cref="NmeaLocationDataSource"/> class.
        /// </summary>
        /// <param name="device">The NMEA device to monitor</param>
        /// <param name="startStopDevice">Whether starting this datasource also controls the underlying NMEA device</param>
        public NmeaLocationDataSource(NmeaParser.NmeaDevice device, bool startStopDevice = true) : this(new GnssMonitor(device), startStopDevice)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NmeaLocationDataSource"/> class.
        /// </summary>
        /// <param name="monitor">The NMEA device to monitor</param>
        /// <param name="startStopDevice">Whether starting this datasource also controls the underlying NMEA device</param>
        public NmeaLocationDataSource(NmeaParser.Gnss.GnssMonitor monitor, bool startStopDevice = true)
        {
            if (monitor == null)
                throw new ArgumentNullException(nameof(monitor));
            this.m_gnssMonitor = monitor;
            m_startStopDevice = startStopDevice;
        }

        protected async override Task OnStartAsync()
        {
            m_gnssMonitor.LocationChanged += OnLocationChanged;
            m_gnssMonitor.LocationLost += OnLocationChanged;
            if (m_startStopDevice && !this.m_gnssMonitor.Device.IsOpen)
                await this.m_gnssMonitor.Device.OpenAsync();

            if (m_gnssMonitor.IsFixValid)
                OnLocationChanged(this, EventArgs.Empty);
        }

        protected override Task OnStopAsync()
        {
            m_gnssMonitor.LocationChanged -= OnLocationChanged;
            m_gnssMonitor.LocationLost -= OnLocationChanged;
            if(m_startStopDevice)
                return m_gnssMonitor.Device.CloseAsync();
            else
                return Task.CompletedTask;
        }

        private void OnLocationChanged(object sender, EventArgs e)
        {
            if (double.IsNaN(m_gnssMonitor.Longitude) || double.IsNaN(m_gnssMonitor.Latitude)) return;
            if (!double.IsNaN(m_gnssMonitor.Course))
                lastCourse = m_gnssMonitor.Course;
            UpdateLocation(new Esri.ArcGISRuntime.Location.Location(
                timestamp: null,
                position: !double.IsNaN(m_gnssMonitor.Altitude) ? new MapPoint(m_gnssMonitor.Longitude, m_gnssMonitor.Latitude, m_gnssMonitor.Altitude, wgs84_ellipsoidHeight) : new MapPoint(m_gnssMonitor.Longitude, m_gnssMonitor.Latitude, SpatialReferences.Wgs84),
                horizontalAccuracy: m_gnssMonitor.HorizontalError,
                verticalAccuracy: m_gnssMonitor.VerticalError,
                velocity: double.IsNaN(m_gnssMonitor.Speed) ? 0 : m_gnssMonitor.Speed * 0.51444444,
                course: lastCourse,
                !m_gnssMonitor.IsFixValid));
        }
    }
}
