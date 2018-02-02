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
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpgsa")]
	[NmeaMessageType("GPGSA")]
	public class Gpgsa : Gsa
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "Enum values matches NMEA spec")]
		public enum Mode : int
		{
			/// <summary>
			/// Not available
			/// </summary>
			NotAvailable = 1,
			/// <summary>
			/// 2D Fix
			/// </summary>
			Fix2D = 2,
			/// <summary>
			/// 3D Fix
			/// </summary>
			Fix3D = 3
		}
	}
}
