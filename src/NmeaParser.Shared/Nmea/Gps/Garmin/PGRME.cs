﻿//
// Copyright (c) 2014 Morten Nielsen
//
// Contributors:
// Stephen Kennedy, Copyright (c) 2016 Gloucester Software Ltd.
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

namespace NmeaParser.Nmea.Gps.Garmin
{
	/// <summary>
	///  Recommended Minimum
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pgrme")]
	[NmeaMessageType("PGRME")]
	public class Pgrme : NmeaMessage
	{
		/// <summary>
		/// Called when the message is being loaded.
		/// </summary>
		/// <param name="message">The NMEA message values.</param>
		protected override void OnLoadMessage(string[] message)
		{
			if (message == null || message.Length < 6)
				throw new ArgumentException("Invalid PGRME", "message"); 
			
			HorizontalError = NmeaMessage.StringToDouble(message[0]);
			HorizontalErrorUnits = message[1];
			VerticalError = NmeaMessage.StringToDouble(message[2]);
			VerticalErrorUnits = message[3];
			SphericalError = NmeaMessage.StringToDouble(message[4]);
			SphericalErrorUnits = message[5];
        }

        /// <summary>
        /// Gets an enumeration value representing the type for this message
        /// </summary>
	    public override NmeaMessageClassType NmeaMessageClassType { get { return NmeaMessageClassType.Pgrme; } }

        /// <summary>
        /// Estimated horizontal position error in meters (HPE)
        /// </summary>
        /// <remarks>Range: 0.0 to 999.9 meters</remarks>
        public double HorizontalError { get; private set; }

		/// <summary>
		/// Horizontal Error unit ('M' for Meters)
		/// </summary>
		public string HorizontalErrorUnits { get; private set; }

		/// <summary>
		/// Estimated vertical position error in meters (VPE)
		/// </summary>
		/// <remarks>Range: 0.0 to 999.9 meters</remarks>
		public double VerticalError { get; private set; }

		/// <summary>
		/// Vertical Error unit ('M' for Meters)
		/// </summary>
		public string VerticalErrorUnits { get; private set; }

		/// <summary>
		/// Overall spherical equivalent position error (EPE)
		/// </summary>
		/// <remarks>Range: 0.0 to 999.9 meters</remarks>
		public double SphericalError { get; private set; }

		/// <summary>
		/// Spherical Error unit ('M' for Meters)
		/// </summary>
		public string SphericalErrorUnits { get; private set; }
	}
}
