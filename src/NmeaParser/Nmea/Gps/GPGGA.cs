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
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpgga")]
	[NmeaMessageType("GPGGA")]
	public class Gpgga : Gga
	{
		/// <summary>
		/// Fix quality
		/// </summary>
		public enum FixQuality : int
		{
			/// <summary>Invalid</summary>
			Invalid = 0,
			/// <summary>GPS</summary>
			GpsFix = 1,
			/// <summary>Differential GPS</summary>
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dgps")]
			DgpsFix = 2,
			/// <summary>Precise Positioning Service</summary>
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pps")]
			PpsFix = 3,
			/// <summary>Real Time Kinematic (Fixed)</summary>
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rtk")]
			Rtk = 4,
			/// <summary>Real Time Kinematic (Floating)</summary>
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rtk")]
			FloatRtk = 5,
			/// <summary>Estimated</summary>
			Estimated = 6,
			/// <summary>Manual input</summary>
			ManualInput = 7,
			/// <summary>Simulation</summary>
			Simulation = 8
		}
	}
}
