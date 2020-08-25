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
using System.Linq;
using NmeaParser.Messages;

namespace NmeaParser.Gnss
{
    /// <summary>
    /// Helper class for monitoring GNSS messages and combine them into a single useful location info
    /// </summary>
    public class GnssMonitor
    {
        private bool m_supportGNMessages; // If device detect GN* messages, ignore all other Talker ID
        private bool m_supportGGaMessages; //If device support GGA, ignore RMC for location

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
        }

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
            AllMessages[message.MessageType] = message;
            
            if (message.TalkerId == NmeaParser.Talker.GlobalNavigationSatelliteSystem)
                m_supportGNMessages = true; // Support for GN* messages detected
            else if (m_supportGNMessages && message.TalkerId != NmeaParser.Talker.GlobalNavigationSatelliteSystem)
                return; // If device supports combined GN* messages, ignore non-GN messages

            if (message is NmeaParser.Messages.Garmin.Pgrme rme)
            {
                HorizontalError = rme.HorizontalError;
                VerticalError = rme.VerticalError;
            }
            else if (message is Gst gst)
            {
                Gst = gst;
                VerticalError = gst.SigmaHeightError;
                HorizontalError = Math.Round(Math.Sqrt(Gst.SigmaLatitudeError * Gst.SigmaLatitudeError + Gst.SigmaLongitudeError * Gst.SigmaLongitudeError), 3);
            }
            else if (message is Rmc rmc)
            {
                Rmc = rmc;
                if (!m_supportGGaMessages)
                {
                    if (Rmc.Active)
                    {
                        lat = Rmc.Latitude;
                        lon = Rmc.Longitude;
                        FixTime = Rmc.FixTime.TimeOfDay;
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
                    Latitude = double.NaN;
                    Longitude = double.NaN;
                    IsFixValid = false;
                }
            }
            else if (message is Gga gga)
            {
                Gga = gga;
                m_supportGGaMessages = true;
                if (gga.Quality != Gga.FixQuality.Invalid)
                {
                    lat = gga.Latitude;
                    lon = gga.Longitude;
                    GeoidHeight = gga.GeoidalSeparation;
                    Altitude = gga.Altitude + gga.GeoidalSeparation; //Convert to ellipsoidal height
                }
                if (gga.Quality == Gga.FixQuality.Invalid || gga.Quality == Gga.FixQuality.Estimated)
                {
                    lostFix = true;
                }
                FixTime = Gga.FixTime;
                isNewFix = true;
            }
            else if (message is Gsa gsa)
            {
                Gsa = gsa;
            }
            else if (message is Vtg vtg)
            {
                Vtg = vtg;
            }
            if (lostFix)
            {
                if (!IsFixValid)
                {
                    IsFixValid = false;
                    LocationLost?.Invoke(this, EventArgs.Empty);
                }
            }
            if (isNewFix)
            {
                Latitude = lat;
                Longitude = lon;
                IsFixValid = true;
                LocationChanged?.Invoke(this, EventArgs.Empty);
            }
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
        public IEnumerable<SatelliteVehicle> Satellites => AllMessages.Values.OfType<Gsv>().SelectMany(s => s.SVs);

        /// <summary>
        /// Gets the number of satellites in the sky
        /// </summary>
        public int SatellitesInView => AllMessages.Values.OfType<Gsv>().Sum(s => s.SatellitesInView);

        /// <summary>
        /// Gets the quality of the current fix
        /// </summary>
        public Gga.FixQuality FixQuality => !IsFixValid ? Gga.FixQuality.Invalid : (Gga?.Quality ?? Gga.FixQuality.GpsFix);

        /// <summary>
        /// Gets a list of all NMEA messages currently part of this location
        /// </summary>
        public Dictionary<string, NmeaMessage> AllMessages { get; } = new Dictionary<string, NmeaMessage>();

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
    }
}
