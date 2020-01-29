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

namespace NmeaParser.Messages
{
    /// <summary>
    /// Global Positioning System DOP and active satellites
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
                throw new ArgumentException("Invalid GSA", "message");

            Mode = message[0] == "A" ? Gsa.ModeSelection.Auto : Gsa.ModeSelection.Manual;
            Fix = (Gsa.FixType)int.Parse(message[1], CultureInfo.InvariantCulture);

            List<int> svs = new List<int>();
            for (int i = 2; i < 14; i++)
            {
                int id = -1;
                if (message[i].Length > 0 && int.TryParse(message[i], out id))
                    svs.Add(id);
            }
            SatelliteIDs = svs.ToArray();

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
        public ModeSelection Mode { get; }

        /// <summary>
        /// Mode
        /// </summary>
        public FixType Fix { get; }

        /// <summary>
        /// ID numbers of satellite vehicles used in the solution.
        /// </summary>
        /// <remarks>
        /// - GPS satellites are identified by their PRN numbers, which range from 1 to 32.
        /// - The numbers 33-64 are reserved for SBAS satellites. The SBAS system PRN numbers are 120-138. The offset from NMEA SBAS SB ID to SBAS PRN number is 87.
        /// A SBAS PRN number of 120 minus 87 yields the SV ID of 33. The addition of87 to the SVID yields the SBAS PRN number.
        /// - The numbers 65-96 are reserved for GLONASS satellites. GLONASS satellites are identified by 64+satellite slot number.
        /// </remarks>
        public int[] SatelliteIDs { get; }

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
            /// Automatic, allowed to automatically switch 2D/3D
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
        public enum FixType : int
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