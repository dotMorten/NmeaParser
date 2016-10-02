//
// Copyright (c) 2016 Frederick Chapleau
//
// Licensed under the Microsoft Public License (Ms-PL) (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://opensource.org/licenses/Ms-PL.html
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser.Nmea.Gps
{
    /// <summary>
    ///  Routes
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpvtg")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [NmeaMessageType("GPVTG")]
    public sealed class Gpvtg : NmeaMessage
    {
        /// <summary>
        /// Called when the message is being loaded.
        /// </summary>
        /// <param name="message">The NMEA message values.</param>
        protected override void OnLoadMessage(string[] message)
        {
            if (message == null || message.Length < 9)
                throw new ArgumentException("Invalid GPVTG", "message");

            if (message[1] != "T") throw  new ArgumentException("Invalid GPVTG, 2nd part should be fixed to 'T'", "message");
            if (message[3] != "M") throw new ArgumentException("Invalid GPVTG, 4th part should be fixed to 'M'", "message");
            if (message[5] != "N") throw new ArgumentException("Invalid GPVTG, 6th part should be fixed to 'N'", "message");
            if (message[7] != "K") throw new ArgumentException("Invalid GPVTG, 8th part should be fixed to 'K'", "message");

            TrueTrackMadeGood = NmeaMessage.StringToDouble(message[0]);
            MageticTrackMadeGood = NmeaMessage.StringToDouble(message[2]);
            SpeedInKnots = NmeaMessage.StringToDouble(message[4]);
            SpeedInKmPerHour = NmeaMessage.StringToDouble(message[6]);
        }

        /// <summary>
        /// Track made good, relative to true North
        /// </summary>
        public double? TrueTrackMadeGood { get; private set; }

        /// <summary>
        /// Track made good, relative to Magetic North
        /// </summary>
        public double? MageticTrackMadeGood { get; private set; }

        /// <summary>
        /// Speed in Knots
        /// </summary>
        public double? SpeedInKnots { get; private set; }

        /// <summary>
        /// Speed in KM per Hour
        /// </summary>
        public double? SpeedInKmPerHour { get; private set; }
    }
}
