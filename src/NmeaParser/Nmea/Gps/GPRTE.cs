﻿//
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
	///  Routes
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gprte")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	[NmeaMessageType("GPRTE")]
	public sealed class Gprte : NmeaMessage, IMultiPartMessage<string>
	{
		/// <summary>
		/// Waypoint tpe
		/// </summary>
		public enum WaypointListType
		{
			/// <summary>
			/// Complete list of waypoints
			/// </summary>
			CompleteWaypointsList,
			/// <summary>
			/// List of remaining waypoints
			/// </summary>
			RemainingWaypointsList
		}
		/// <summary>
		/// Called when the message is being loaded.
		/// </summary>
		/// <param name="message">The NMEA message values.</param>
		protected override void OnLoadMessage(string[] message)
		{
			if (message == null || message.Length < 4)
				throw new ArgumentException("Invalid GPRTE", "message");

			TotalMessages = int.Parse(message[0], CultureInfo.InvariantCulture);
			MessageNumber = int.Parse(message[1], CultureInfo.InvariantCulture);
			ListType = message[2] == "c" ? WaypointListType.CompleteWaypointsList : WaypointListType.RemainingWaypointsList;
			RouteId = message[3];
			Waypoints = message.Skip(4).ToArray();
		}

		/// <summary>
		/// Total number of messages of this type in this cycle
		/// </summary>
		public int TotalMessages { get; private set; }

		/// <summary>
		/// Message number
		/// </summary>
		public int MessageNumber { get; private set; }

		/// <summary>
		/// Gets the type of the list.
		/// </summary>
		public WaypointListType ListType { get; private set; }

		/// <summary>
		/// Gets the route identifier.
		/// </summary>
		public string RouteId { get; private set; }
		
		/// <summary>
		/// Waypoints
		/// </summary>
		public IReadOnlyList<string> Waypoints { get; private set; }

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns> A System.Collections.Generic.IEnumerator{T} that can be used to iterate through the collection.</returns>
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			foreach (string waypoint in Waypoints)
				yield return waypoint;
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns> An System.Collections.IEnumerator object that can be used to iterate through the collection.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}
}
