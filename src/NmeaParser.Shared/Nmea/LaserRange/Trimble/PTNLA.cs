﻿//
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser.Nmea.LaserRange.Trimble
{
	/// <summary>
	/// Burden finder
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ptnla")]
	[NmeaMessageType("PTNLA")]
	public class Ptnla : LaserRangeMessage
    {

        /// <summary>
        /// Gets an enumeration value representing the type for this message
        /// </summary>
	    public override NmeaMessageClassType NmeaMessageClassType { get { return NmeaMessageClassType.Ptnla; } }
    }
}
