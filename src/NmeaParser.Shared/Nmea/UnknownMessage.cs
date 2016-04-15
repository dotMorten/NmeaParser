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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser.Nmea
{
	/// <summary>
	/// Represents an unknown message type
	/// </summary>
	public class UnknownMessage : NmeaMessage
	{
		/// <summary>
		/// Gets the nmea value array.
		/// </summary>
		public IReadOnlyList<string> Values { get { return base.MessageParts; } }

        /// <summary>
        /// Gets an enumeration value representing the type for this message
        /// </summary>
	    public override NmeaMessageClassType NmeaMessageClassType { get { return NmeaMessageClassType.UnknownMessage; } }

        /// <summary>
        /// Called when the message is being loaded.
        /// </summary>
        /// <param name="message">The NMEA message values.</param>
        protected override void OnLoadMessage(string[] message)
		{
		}
	}
}
