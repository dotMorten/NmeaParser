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

namespace NmeaParser.Nmea.Trimble.LaserRange
{
	/// <summary>
	/// Tree Measurement
	/// </summary>
	[NmeaMessageType(Type = "PTNLB")]
	public class Ptnlb : NmeaMessage
	{
		protected override void LoadMessage(string[] message)
		{
			TreeHeight = message[0];
			MeasuredTreeHeight = double.Parse(message[1], CultureInfo.InvariantCulture);
			MeasuredTreeHeightUnits = message[2][0];
			TreeDiameter = message[3];
			MeasuredTreeDiameter = double.Parse(message[4], CultureInfo.InvariantCulture);
			MeasuredTreeDiameterUnits = message[5][0];
		}

		public string TreeHeight { get; private set; }

		public double MeasuredTreeHeight { get; private set; }

		public char MeasuredTreeHeightUnits { get; private set; }

		public string TreeDiameter { get; private set; }

		public double MeasuredTreeDiameter { get; private set; }

		public char MeasuredTreeDiameterUnits { get; private set; }

		//more to do...
	
	}
}
