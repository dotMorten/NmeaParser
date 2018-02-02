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

using NmeaParser.Nmea;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser.Nmea.LaserRange
{
	/// <summary>
	/// Laser Range Measurement
	/// </summary>
	public abstract class LaserRangeMessage : NmeaMessage
	{
		/// <summary>
		/// Called when the message is being loaded.
		/// </summary>
		/// <param name="message">The NMEA message values.</param>
		protected override void OnLoadMessage(string[] message)
		{
			if (message == null || message.Length < 9)
				throw new ArgumentException("Invalid Laser Range Message", "message"); 
			
			HorizontalVector = message[0];
			HorizontalDistance = double.Parse(message[1], CultureInfo.InvariantCulture);
			HorizontalDistanceUnits = message[2][0];
			HorizontalAngle = double.Parse(message[3], CultureInfo.InvariantCulture);
			HorizontalAngleUnits = message[4][0];
			VerticalAngle = double.Parse(message[5], CultureInfo.InvariantCulture);
			VerticalAngleUnits = message[6][0];
			SlopeDistance = double.Parse(message[7], CultureInfo.InvariantCulture);
			SlopeDistanceUnits = message[8][0];
		}

		/// <summary>
		/// Gets the horizontal vector.
		/// </summary>
		public string HorizontalVector { get; private set; }

		/// <summary>
		/// Gets the horizontal distance.
		/// </summary>
		public double HorizontalDistance { get; private set; }

		/// <summary>
		/// Gets the units of the <see cref="HorizontalDistance"/> value.
		/// </summary>
		public char HorizontalDistanceUnits { get; private set; }

		/// <summary>
		/// Gets the horizontal angle.
		/// </summary>
		public double HorizontalAngle { get; private set; }

		/// <summary>
		/// Gets the units of the <see cref="HorizontalAngle"/> value.
		/// </summary>
		public char HorizontalAngleUnits { get; private set; }

		/// <summary>
		/// Gets the vertical angle.
		/// </summary>
		public double VerticalAngle { get; private set; }

		/// <summary>
		/// Gets the units of the <see cref="VerticalAngle"/> value.
		/// </summary>
		public char VerticalAngleUnits { get; private set; }

		/// <summary>
		/// Gets the slope distance.
		/// </summary>
		public double SlopeDistance { get; private set; }

		/// <summary>
		/// Gets the units of the <see cref="SlopeDistance"/> value.
		/// </summary>
		public char SlopeDistanceUnits { get; private set; }
	}
}
