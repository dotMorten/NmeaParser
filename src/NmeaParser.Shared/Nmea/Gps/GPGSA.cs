﻿﻿//
// Copyright (c) 2014 Morten Nielsen
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
	///  Global Positioning System Fix Data
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpgsa")]
	[NmeaMessageType("GPGSA")]
	public class Gpgsa : Gsa
	{
		/// <summary>
		/// Called when the message is being loaded.
		/// </summary>
		/// <param name="message">The NMEA message values.</param>
		protected override void OnLoadMessage(string[] message)
		{
            base.OnLoadMessage(message);
            Talker = TalkerId.GP;
		}

        /// <summary>
        /// Operation mode. Redirecting to OperationMode for backward compatibility.
        /// </summary>
        public ModeSelection GpsMode 
        { 
            get { return OperationMode; } 
        }
	}
}
