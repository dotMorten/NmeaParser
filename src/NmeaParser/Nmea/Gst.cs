﻿//  *******************************************************************************
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

namespace NmeaParser.Nmea
{
    /// <summary>
    /// Pseudorange error statistics
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpgst")]
    [NmeaMessageType("--GST")]
    public class Gst : NmeaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gst"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Gst(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 8)
                throw new ArgumentException("Invalid GPGST", "message");
            FixTime = StringToTimeSpan(message[0]);
            Rms = NmeaMessage.StringToDouble(message[1]);
            SemiMajorError = NmeaMessage.StringToDouble(message[2]);
            SemiMinorError = NmeaMessage.StringToDouble(message[3]);
            ErrorOrientation = NmeaMessage.StringToDouble(message[4]);
            SigmaLatitudeError = NmeaMessage.StringToDouble(message[5]);
            SigmaLongitudeError = NmeaMessage.StringToDouble(message[6]);
            SigmaHeightError = NmeaMessage.StringToDouble(message[7]);
        }

        /// <summary>
        /// UTC of position fix
        /// </summary>
        public TimeSpan FixTime { get; }

        /// <summary>
        /// RMS value of the standard deviation of the range inputs in the navigation process. Range inputs include pseudoranges and DGNSS corrections.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rms")]
        public double Rms { get; }

        /// <summary>
        /// Standard deviation of semi-major axis of error ellipse in meters.
        /// </summary>
        public double SemiMajorError { get; }

        /// <summary>
        /// Standard deviation of semi-minor axis of error ellipse in meters.
        /// </summary>
        public double SemiMinorError { get; }

        /// <summary>
        /// Orientation of semi-major axis of error ellipse (degrees from true north).
        /// </summary>
        public double ErrorOrientation { get; }

        /// <summary>
        /// Standard deviation of latitude error in meters.
        /// </summary>
        public double SigmaLatitudeError { get; }

        /// <summary>
        /// Standard deviation of longitude error in meters.
        /// </summary>
        public double SigmaLongitudeError { get; }

        /// <summary>
        /// Standard deviation of altitude error in meters.
        /// </summary>
        public double SigmaHeightError { get; }
    }
}
