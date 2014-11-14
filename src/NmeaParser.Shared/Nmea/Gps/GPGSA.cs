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
	///  Global Positioning System Fix Data
	/// </summary>
	[NmeaMessageType(Type = "GPGSA")]
	public class Gpgsa : NmeaMessage
	{
		/// <summary>
		/// Mode selection
		/// </summary>
		public enum ModeSelection
		{
			/// <summary>
			/// Auto
			/// </summary>
			Auto,
			/// <summary>
			/// Manual mode
			/// </summary>
			Manual,
		}
		/// <summary>
		/// Fix Mode
		/// </summary>
		public enum Mode : int
		{
			/// <summary>
			/// Not available
			/// </summary>
			NotAvailable = 1,
			/// <summary>
			/// 2D Fix
			/// </summary>
			_2D = 2,
			/// <summary>
			/// 3D Fix
			/// </summary>
			_3D = 3
		}

		/// <summary>
		/// Called when the message is being loaded.
		/// </summary>
		/// <param name="message">The NMEA message values.</param>
		protected override void OnLoadMessage(string[] message)
		{
			GpsMode = message[0] == "A" ? ModeSelection.Auto : ModeSelection.Manual;
			FixMode = (Mode)int.Parse(message[1]);

			List<int> svs = new List<int>();
			for (int i = 2; i < 14; i++)
			{
				int id = -1;
				if (message[i].Length > 0 && int.TryParse(message[i], out id))
					svs.Add(id);
			}
			SVs = svs.ToArray();

			double tmp;
			if (double.TryParse(message[14], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
				PDop = tmp;
			else
				PDop = double.NaN;

			if (double.TryParse(message[15], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
				HDop = tmp;
			else
				HDop = double.NaN;

			if (double.TryParse(message[16], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
				VDop = tmp;
			else
				VDop = double.NaN;
		}

		/// <summary>
		/// Mode
		/// </summary>
		public ModeSelection GpsMode { get; private set; }

		/// <summary>
		/// Mode
		/// </summary>
		public Mode FixMode { get; private set; }

		/// <summary>
		/// IDs of SVs used in position fix
		/// </summary>
		public int[] SVs { get; private set; }

		/// <summary>
		/// Dilution of precision
		/// </summary>
		public double PDop { get; private set; }

		/// <summary>
		/// Horizontal dilution of precision
		/// </summary>
		public double HDop { get; private set; }

		/// <summary>
		/// Vertical dilution of precision
		/// </summary>
		public double VDop { get; private set; }
	}
}
