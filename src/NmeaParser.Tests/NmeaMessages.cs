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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using NmeaParser.Nmea;
using NmeaParser.Nmea.Gps;

namespace NmeaParser.Tests
{
    [TestClass]
    public class NmeaMessages
    {
        [TestMethod]
        public void TestGprmc()
        {
			string input = "$GPRMC,123519,A,4807.038,S,01131.000,W,022.4,084.4,230313,003.1,W*6E";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(Gprmc));
			Gprmc rmc = (Gprmc)msg;
			Assert.AreEqual(new DateTime(23, 03, 2013, 12, 35, 19, DateTimeKind.Utc), rmc.FixTime);
        }

		[TestMethod]
		public void TestPtlna()
		{
			string input = "$PTNLA,HV,002.94,M,288.1,D,008.6,D,002.98,M*74";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(Nmea.Trimble.LaserRange.Ptnla));
			Nmea.Trimble.LaserRange.Ptnla ptlna = (Nmea.Trimble.LaserRange.Ptnla)msg;
			Assert.AreEqual(2.94, ptlna.HorizontalDistance);
			Assert.AreEqual('M', ptlna.HorizontalDistanceUnits);
			Assert.AreEqual(288.1, ptlna.HorizontalAngle);
			Assert.AreEqual('D', ptlna.HorizontalAngleUnits);
			Assert.AreEqual(8.6, ptlna.VerticalAngle);
			Assert.AreEqual('D', ptlna.VerticalAngleUnits);
			Assert.AreEqual(2.98, ptlna.SlopeDistance);
			Assert.AreEqual('M', ptlna.SlopeDistanceUnits);
		}
	}
}
