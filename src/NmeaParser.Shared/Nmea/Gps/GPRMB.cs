//
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

namespace NmeaParser.Nmea.Gps
{
	/// <summary>
	/// Recommended minimum navigation information
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gprmb")]
	[NmeaMessageType("GPRMB")]
	public class Gprmb : NmeaMessage
	{
		/// <summary>
		/// Data status
		/// </summary>
		public enum DataStatus
		{
			/// <summary>
			/// Ok
			/// </summary>
			Ok,
			/// <summary>
			/// Warning
			/// </summary>
			Warning
		}
		/// <summary>
		/// Called when the message is being loaded.
		/// </summary>
		/// <param name="message">The NMEA message values.</param>
		protected override void OnLoadMessage(string[] message)
		{
			if (message == null || message.Length < 13)
				throw new ArgumentException("Invalid GPRMB", "message"); 
			
			Status = message[0] == "A" ? DataStatus.Ok : Gprmb.DataStatus.Warning;
			double tmp;
			if (double.TryParse(message[1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
			{
				CrossTrackError = tmp;

				if (message[2] == "L") //Steer left
					CrossTrackError *= -1;
			}
			else
				CrossTrackError = double.NaN;

			if(message[3].Length > 0)
				OriginWaypointId = int.Parse(message[3], CultureInfo.InvariantCulture);
			if (message[3].Length > 0)
				DestinationWaypointId = int.Parse(message[4], CultureInfo.InvariantCulture);
			DestinationLatitude = NmeaMessage.StringToLatitude(message[5], message[6]);
			DestinationLongitude = NmeaMessage.StringToLongitude(message[7], message[8]);
			if (double.TryParse(message[9], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
				RangeToDestination = tmp;
			else
				RangeToDestination = double.NaN;
			if (double.TryParse(message[10], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
				TrueBearing = tmp;
			else
				TrueBearing = double.NaN;
			if (double.TryParse(message[11], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
				Velocity = tmp;
			else
				Velocity = double.NaN;
			Arrived = message[12] == "A";
        }

        /// <summary>
        /// Gets an enumeration value representing the type for this message
        /// </summary>
	    public override NmeaMessageClassType NmeaMessageClassType { get { return NmeaMessageClassType.Gprmb; } }

        /// <summary>
        /// Data Status
        /// </summary>
        public DataStatus Status { get; private set; }

		/// <summary>
		/// Cross-track error (steer left when negative, right when positive)
		/// </summary>
		public double CrossTrackError { get; private set; }
		
		/// <summary>
		/// Origin waypoint ID
		/// </summary>
		public double OriginWaypointId { get; private set; }

		/// <summary>
		/// Destination waypoint ID
		/// </summary>
		public double DestinationWaypointId { get; private set; }

		/// <summary>
		/// Destination Latitude
		/// </summary>
		public double DestinationLatitude { get; private set; }

		/// <summary>
		/// Destination Longitude
		/// </summary>
		public double DestinationLongitude { get; private set; }

		/// <summary>
		/// Range to destination in nautical miles
		/// </summary>
		public double RangeToDestination { get; private set; }

		/// <summary>
		/// True bearing to destination
		/// </summary>
		public double TrueBearing { get; private set; }

		/// <summary>
		/// Velocity towards destination in knots
		/// </summary>
		public double Velocity { get; private set; }

		/// <summary>
		/// Arrived (<c>true</c> if arrived)
		/// </summary>
		public bool Arrived { get; private set; }
	}
}
