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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using NmeaParser.Messages;

namespace NmeaParser.Gnss
{
    /// <summary>
    /// Helper class for monitoring GNSS messages and combine them into a single useful location info
    /// </summary>
    public class GnssMonitor : INotifyPropertyChanged
    {
        private bool m_supportGNMessages; // If device detect GN* messages, ignore all other Talker ID
        private bool m_supportGGaMessages; //If device support GGA, ignore RMC for location
        private Dictionary<string, NmeaMessage> m_allMessages { get; } = new Dictionary<string, NmeaMessage>();
        private object m_lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="GnssMonitor"/> class.
        /// </summary>
        /// <param name="device">The NMEA device to monitor for GNSS messages</param>
        public GnssMonitor(NmeaDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            Device = device;
            Device.MessageReceived += NmeaMessageReceived;
            SynchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Gets or sets the syncronization context that <see cref="PropertyChanged"/> should be fired on
        /// </summary>
        /// <remarks>
        /// The default is the context this thread was created monitor was created on, but for use in UI applications, 
        /// it can be beneficial to ensure this is the UI Thread. You can also set this to <c>null</c> for best performance
        /// </remarks>
        public SynchronizationContext? SynchronizationContext { get; set; }

        /// <summary>
        /// Gets the NMEA device that is being monitored
        /// </summary>
        public NmeaDevice Device { get; }

        private void NmeaMessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs e)
        {
            OnMessageReceived(e.Message);
        }

        /// <summary>
        /// Called when a message is received.
        /// </summary>
        /// <param name="message">The NMEA message that was received</param>
        protected virtual void OnMessageReceived(NmeaMessage message)
        {
            bool isNewFix = false;
            bool lostFix = false;
            double lat = 0;
            double lon = 0;
            List<string> properties = new List<string>();
            lock (m_lock)
            {
                if(m_allMessages.ContainsKey(message.MessageType) && m_allMessages[message.MessageType].Equals(message))
                        return; // Nothing to update/notify
                m_allMessages[message.MessageType] = message;
            }
            properties.Add(nameof(AllMessages));
            if (message.TalkerId == NmeaParser.Talker.GlobalNavigationSatelliteSystem)
                m_supportGNMessages = true; // Support for GN* messages detected
            else if (m_supportGNMessages && message.TalkerId != NmeaParser.Talker.GlobalNavigationSatelliteSystem && !(message is Gsv))
                return; // If device supports combined GN* messages, ignore non-GN messages, except for Gsv

            if (message is NmeaParser.Messages.Garmin.Pgrme rme)
            {
                if (rme.HorizontalError != HorizontalError)
                {
                    properties.Add(nameof(HorizontalError));
                    HorizontalError = rme.HorizontalError;
                }
                if (rme.VerticalError != VerticalError)
                {
                    VerticalError = rme.VerticalError;
                    properties.Add(nameof(VerticalError));
                }
            }
            else if (message is Gst gst)
            {
                Gst = gst;
                properties.Add(nameof(Gst));
                var error = Math.Round(Math.Sqrt(Gst.SigmaLatitudeError * Gst.SigmaLatitudeError + Gst.SigmaLongitudeError * Gst.SigmaLongitudeError), 3);
                if (error != HorizontalError)
                {
                    HorizontalError = error;
                    properties.Add(nameof(HorizontalError));
                }
                if (VerticalError != gst.SigmaHeightError)
                {
                    VerticalError = gst.SigmaHeightError;
                    properties.Add(nameof(VerticalError));
                }
            }
            else if (message is Rmc rmc)
            {
                if (Speed != rmc.Speed)
                    properties.Add(nameof(Speed));
                if (Course != rmc.Course)
                    properties.Add(nameof(Course));
                Rmc = rmc;
                properties.Add(nameof(Rmc));
                if (!m_supportGGaMessages)
                {
                    if (Rmc.Active)
                    {
                        lat = Rmc.Latitude;
                        lon = Rmc.Longitude;
                        if (FixTime != Rmc.FixTime.TimeOfDay)
                        {
                            FixTime = Rmc.FixTime.TimeOfDay;
                            properties.Add(nameof(FixTime));
                        }
                        isNewFix = true;
                    }
                    else
                    {
                        lostFix = true;
                    }
                }
            }
            else if (message is Dtm dtm)
            {
                if (Dtm?.Checksum != dtm.Checksum)
                {
                    // Datum change
                    Dtm = dtm;
                    properties.Add(nameof(Dtm));
                    Latitude = double.NaN;
                    Longitude = double.NaN;
                    IsFixValid = false;
                    properties.Add(nameof(Dtm));
                    properties.Add(nameof(Datum));
                    properties.Add(nameof(Latitude));
                    properties.Add(nameof(Longitude));
                    properties.Add(nameof(IsFixValid));
                }
            }
            else if (message is Gga gga)
            {
                if (gga.Hdop != Hdop)
                    properties.Add(nameof(Hdop));
                if (gga.Quality != FixQuality)
                    properties.Add(nameof(FixQuality));
                Gga = gga;
                properties.Add(nameof(Gga));
                m_supportGGaMessages = true;
                if (gga.Quality != Gga.FixQuality.Invalid)
                {
                    lat = gga.Latitude;
                    lon = gga.Longitude;
                    GeoidHeight = gga.GeoidalSeparation;
                    properties.Add(nameof(GeoidHeight));
                    Altitude = gga.Altitude + gga.GeoidalSeparation; //Convert to ellipsoidal height
                    properties.Add(nameof(Altitude));
                }
                if (gga.Quality == Gga.FixQuality.Invalid || gga.Quality == Gga.FixQuality.Estimated)
                {
                    lostFix = true;
                }
                if (FixTime != Gga.FixTime)
                {
                    FixTime = Gga.FixTime;
                    properties.Add(nameof(FixTime));
                }
                isNewFix = true;
            }
            else if (message is Gsa gsa)
            {
                if (gsa.Hdop != Hdop)
                    properties.Add(nameof(Hdop));
                if (gsa.Pdop != Pdop)
                    properties.Add(nameof(Pdop));
                if (gsa.Vdop != Vdop)
                    properties.Add(nameof(Vdop));
                Gsa = gsa;
                properties.Add(nameof(Gsa));
            }
            else if (message is Vtg vtg)
            {
                if (Speed != vtg.SpeedKnots)
                    properties.Add(nameof(Speed));
                Vtg = vtg;
                properties.Add(nameof(Vtg));
            }
            else if (message is Gsv)
            {
                properties.Add(nameof(Satellites));
                properties.Add(nameof(SatellitesInView));
            }
            if (lostFix)
            {
                if (!IsFixValid)
                {
                    IsFixValid = false;
                    properties.Add(nameof(IsFixValid));
                    properties.Add(nameof(FixQuality));
                    LocationLost?.Invoke(this, EventArgs.Empty);
                }
            }
            if (isNewFix)
            {
                if (Latitude != lat)
                {
                    properties.Add(nameof(Latitude));
                    Latitude = lat;
                }
                if (Longitude != lon)
                {
                    properties.Add(nameof(Longitude));
                    Longitude = lon;
                }
                if (!IsFixValid)
                {
                    properties.Add(nameof(IsFixValid));
                    if (Gga == null)
                        properties.Add(nameof(FixQuality));
                    IsFixValid = true;
                }
                LocationChanged?.Invoke(this, EventArgs.Empty);
            }
            if (properties.Count > 0)
                OnPropertyChanged(properties);
        }

        /// <summary>
        /// Gets a value indicating whether the current fix is valid.
        /// </summary>
        /// <remarks>
        /// If <c>false</c> the provided values like <see cref="Latitude"/> and <see cref="Longitude"/> are no longer current and reflect the last known location.
        /// </remarks>
        /// <seealso cref="LocationLost"/>
        public bool IsFixValid { get; private set; }

        /// <summary>
        /// Gets the latitude for the current or last known location.
        /// </summary>
        /// <seealso cref="IsFixValid"/>
        /// <seealso cref="Longitude"/>
        public double Latitude { get; private set; } = double.NaN;

        /// <summary>
        /// Gets the longitude for the current or last known location.
        /// </summary>
        /// <seealso cref="IsFixValid"/>
        /// <seealso cref="Latitude"/>
        public double Longitude { get; private set; } = double.NaN;

        /// <summary>
        /// Gets the geight above the ellipsoid
        /// </summary>
        public double Altitude { get; private set; } = double.NaN;
        /// <summary>
        /// Gets the Geoid Height. Add this value to <see cref="Altitude"/> to get the Geoid heights which is roughly MSL heights.
        /// </summary>
        public double GeoidHeight { get; private set; } = double.NaN;

        /// <summary>
        /// Gets the speed in knots
        /// </summary>
        public double Speed => Rmc?.Speed ?? Vtg?.SpeedKnots ?? double.NaN;

        /// <summary>
        /// Gets the current cource
        /// </summary>
        public double Course => Rmc?.Course ?? double.NaN;

        /// <summary>
        /// Gets an estimate of the horizontal error in meters
        /// </summary>
        public double HorizontalError { get; private set; } = double.NaN;

        /// <summary>
        /// Gets an estimate of the vertical error in meters
        /// </summary>
        public double VerticalError { get; private set; } = double.NaN;

        /// <summary>
        /// Gets the horizontal dilution of precision
        /// </summary>
        public double Hdop => Gsa?.Hdop ?? Gga?.Hdop ?? double.NaN;

        /// <summary>
        /// Gets the 3D point dilution of precision
        /// </summary>
        public double Pdop => Gsa?.Pdop ?? double.NaN;

        /// <summary>
        /// Gets the vertical dilution of precision
        /// </summary>
        public double Vdop => Gsa?.Vdop ?? double.NaN;

        /// <summary>
        /// Gets the latest known GSA message.
        /// </summary>
        public Gsa? Gsa { get; private set; }

        /// <summary>
        /// Gets the latest known GGA message.
        /// </summary>
        public Gga? Gga { get; private set; }

        /// <summary>
        /// Gets the latest known RMC message.
        /// </summary>
        public Rmc? Rmc { get; private set; }

        /// <summary>
        /// Gets the latest known GST message.
        /// </summary>
        public Gst? Gst { get; private set; }

        /// <summary>
        /// Gets the latest known DTM message.
        /// </summary>
        public Dtm? Dtm { get; private set; }

        /// <summary>
        /// Gets the latest known VTG message.
        /// </summary>
        public Vtg? Vtg { get; private set; }

        /// <summary>
        /// Gets the current fix time
        /// </summary>
        public TimeSpan? FixTime { get; private set; }

        /// <summary>
        /// Gets a list of satellite vehicles in the sky
        /// </summary>
        public IEnumerable<SatelliteVehicle> Satellites
        {
            get
            {
                lock (m_lock)
                {
                    return m_allMessages.Values.OfType<Gsv>().SelectMany(s => s.SVs).ToArray();
                }
            }
        }

        /// <summary>
        /// Gets the number of satellites in the sky
        /// </summary>
        public int SatellitesInView
        {
            get
            {
                lock (m_lock)
                {
                    return m_allMessages.Values.OfType<Gsv>().Sum(s => s.SatellitesInView);
                }
            }
        }


        /// <summary>
        /// Gets the quality of the current fix
        /// </summary>
        public Gga.FixQuality FixQuality => !IsFixValid ? Gga.FixQuality.Invalid : (Gga?.Quality ?? Gga.FixQuality.GpsFix);


        /// <summary>
        /// Gets a list of all NMEA messages currently part of this location
        /// </summary>
        public IEnumerable<KeyValuePair<string, NmeaMessage>> AllMessages
        {
            get
            {
                lock (m_lock)
                {
                    return m_allMessages.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating the current Datum being used.
        /// </summary>
        public string Datum
        {
            get
            {
                if (Dtm == null)
                    return "WGS84";
                switch (Dtm.ReferenceDatumCode)
                {
                    case "W84": return "WGS84";
                    case "W72": return "WGS72";
                    case "S85": return "SGS85";
                    case "P90": return "PE90";
                    default: return Dtm.ReferenceDatumCode;
                }
            }
        }

        /// <summary>
        /// Raised when a new location has been updated
        /// </summary>
        public event EventHandler? LocationChanged;

        /// <summary>
        /// Raised if location tracking was lost
        /// </summary>
        /// <seealso cref="IsFixValid"/>
        public event EventHandler? LocationLost;

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(IEnumerable<string> properties)
        {
            if (PropertyChanged == null)
                return;
            if (SynchronizationContext != null)
            {
                SynchronizationContext.Post((d) =>
                {
                    foreach (string propertyName in (IEnumerable<string>)d)
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }, properties);
            }
            else
            {
                foreach (string propertyName in properties)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
