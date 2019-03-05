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
using System.Globalization;

namespace NmeaParser.Nmea
{
    /// <summary>
    ///  Global Positioning System Fix Data
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gsa")]
    [NmeaMessageType("--GSA")]
    public class Gsa : NmeaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gsa"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Gsa(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 17)
                throw new ArgumentException("Invalid GPGSA", "message");

            GpsMode = message[0] == "A" ? Gsa.ModeSelection.Auto : Gsa.ModeSelection.Manual;
            FixMode = (Gsa.Mode)int.Parse(message[1], CultureInfo.InvariantCulture);

            List<int> svs = new List<int>();
            for (int i = 2; i < 14; i++)
            {
                int id = -1;
                if (message[i].Length > 0 && int.TryParse(message[i], out id))
                    svs.Add(id);
            }
            SVs = svs.ToArray();

            double tmp;
            if (double.TryParse(message[14], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
                Pdop = tmp;
            else
                Pdop = double.NaN;

            if (double.TryParse(message[15], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
                Hdop = tmp;
            else
                Hdop = double.NaN;

            if (double.TryParse(message[16], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
                Vdop = tmp;
            else
                Vdop = double.NaN;
        }

        /// <summary>
        /// Mode
        /// </summary>
        public ModeSelection GpsMode { get; }

        /// <summary>
        /// Mode
        /// </summary>
        public Mode FixMode { get; }

        /// <summary>
        /// IDs of SVs used in position fix
        /// </summary>
        public IReadOnlyList<int> SVs { get; }

        /// <summary>
        /// Dilution of precision
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pdop")]
        public double Pdop { get; }

        /// <summary>
        /// Horizontal dilution of precision
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hdop")]
        public double Hdop { get; }

        /// <summary>
        /// Vertical dilution of precision
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vdop")]
        public double Vdop { get; }

        /// <summary>
        /// Mode selection
        /// </summary>
        public enum ModeSelection
        {
            /// <summary>
            /// Auto
            /// </summary>
            Auto,
            /// <summary>
            /// Manual mode
            /// </summary>
            Manual,
        }
        /// <summary>
        /// Fix Mode
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "Enum values matches NMEA spec")]
        public enum Mode : int
        {
            /// <summary>
            /// Not available
            /// </summary>
            NotAvailable = 1,
            /// <summary>
            /// 2D Fix
            /// </summary>
            Fix2D = 2,
            /// <summary>
            /// 3D Fix
            /// </summary>
            Fix3D = 3
        }
    }
}