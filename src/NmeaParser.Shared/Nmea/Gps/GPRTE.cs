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
	[NmeaMessageType(Type = "GPRTE")]
	public sealed class Gprte : NmeaMessage, IMultiPartMessage<string>
	{
		public enum WaypointListType
		{
			CompleteWaypointsList,
			RemainingWaypointsList
		}
		protected override void LoadMessage(string[] message)
		{
			TotalMessages = int.Parse(message[0]);
			MessageNumber = int.Parse(message[1]);
			ListType = message[2] == "c" ? WaypointListType.CompleteWaypointsList : WaypointListType.RemainingWaypointsList;
			RouteID = message[3];
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

		public WaypointListType ListType { get; private set; }
		
		public string RouteID { get; set; }
		
		/// <summary>
		/// Waypoints
		/// </summary>
		public string[] Waypoints { get; private set; }

		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			foreach (string waypoint in Waypoints)
				yield return waypoint;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}
}
