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

namespace NmeaParser.Nmea.LaserRange.Trimble
{
	/// <summary>
	/// Tree Measurement
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ptnlb")]
	[NmeaMessageType("PTNLB")]
	public class Ptnlb : NmeaMessage
	{
		/// <summary>
		/// Called when the message is being loaded.
		/// </summary>
		/// <param name="message">The NMEA message values.</param>
		protected override void OnLoadMessage(string[] message)
		{
			if (message == null || message.Length < 6)
				throw new ArgumentException("Invalid PTNLB", "message"); 
			
			TreeHeight = message[0];
			MeasuredTreeHeight = double.Parse(message[1], CultureInfo.InvariantCulture);
			MeasuredTreeHeightUnits = message[2][0];
			TreeDiameter = message[3];
			MeasuredTreeDiameter = double.Parse(message[4], CultureInfo.InvariantCulture);
			MeasuredTreeDiameterUnits = message[5][0];
		}

		/// <summary>
		/// Gets the height of the tree.
		/// </summary>
		public string TreeHeight { get; private set; }

		/// <summary>
		/// Gets the message height of the tree.
		/// </summary>
		public double MeasuredTreeHeight { get; private set; }

		/// <summary>
		/// Gets the units of the <see cref="MeasuredTreeHeight"/> value.
		/// </summary>
		public char MeasuredTreeHeightUnits { get; private set; }

		/// <summary>
		/// Gets the tree diameter.
		/// </summary>
		public string TreeDiameter { get; private set; }

		/// <summary>
		/// Gets the measured tree diameter.
		/// </summary>
		public double MeasuredTreeDiameter { get; private set; }

		/// <summary>
		/// Gets the units of the <see cref="MeasuredTreeDiameter"/> value.
		/// </summary>
		public char MeasuredTreeDiameterUnits { get; private set; }

		//more to do...
	
	}
}
