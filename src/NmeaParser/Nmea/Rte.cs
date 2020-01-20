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
using System.Linq;

namespace NmeaParser.Messages
{
    /// <summary>
    /// Routes
    /// </summary>
    /// <remarks>
    /// Waypoint identifiers, listed in order with starting waypoint first, for the identified route. Two modes of
    /// transmission are provided: 'c' indicates that the complete list of waypoints in the route are being
    /// transmitted; 'w' indicates a working route where the first listed waypoint is always the last waypoint
    /// that had been reached (FROM), while the second listed waypoint is always the waypoint that the vessel is
    /// currently heading for (TO), the remaining list of waypoints represents the remainder of the route. 
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gprte")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [NmeaMessageType("--RTE")]
    public sealed class Rte : NmeaMultiSentenceMessage, IEnumerable<string>
    {
        private readonly List<string> _waypoints = new List<string>();
        
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
        /// Initializes a new instance of the <see cref="Rte"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Rte(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 4)
                throw new ArgumentException("Invalid GPRTE", "message");
            ListType = message[2] == "c" ? WaypointListType.CompleteWaypointsList : WaypointListType.RemainingWaypointsList;
            RouteId = message[3];
        }

        /// <inheritdoc />
        protected override bool ParseSentences(Talker talker, string[] message)
        {
            if (MessageParts[2] != message[2] || MessageParts[3] != message[3])
                return false;
            _waypoints.AddRange(message.Skip(4));
            return true;
        }

        /// <summary>
        /// Gets the type of the list.
        /// </summary>
        public WaypointListType ListType { get; }

        /// <summary>
        /// Gets the route identifier.
        /// </summary>
        public string RouteId { get; }

        /// <summary>
        /// Waypoints
        /// </summary>
        public IReadOnlyList<string> Waypoints => _waypoints.AsReadOnly();

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
