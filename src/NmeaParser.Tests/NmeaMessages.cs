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
			string input = "$GPRMC,123519,A,4807.038,S,01131.000,W,022.4,084.4,230313,003.1,W*6A";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(Gprmc));
			Gprmc rmc = (Gprmc)msg;
			Assert.AreEqual(new DateTime(2013, 03, 23, 12, 35, 19, DateTimeKind.Utc), rmc.FixTime);
			Assert.AreEqual(-48.1173, rmc.Latitude);
			Assert.AreEqual(-11.516666666666667, rmc.Longitude, 0.0000000001);
        }

		[TestMethod]
		public void TestGpgga()
		{
			string input = "$GPGGA,235236,3925.9479,N,11945.9211,W,1,10,0.8,1378.0,M,-22.1,M,,*46";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(Gpgga));
			Gpgga gga = (Gpgga)msg;
			Assert.AreEqual(new TimeSpan(23, 52, 36), gga.TimeSinceLastDgpsUpdate);
			Assert.AreEqual(39.432465, gga.Latitude);
			Assert.AreEqual(-119.7653516666666667, gga.Longitude, 0.0000000001);
			Assert.AreEqual(NmeaParser.Nmea.Gps.Gpgga.FixQuality.GpsFix, gga.Quality);
			Assert.AreEqual(10, gga.NumberOfSatellites);
			Assert.AreEqual(.8, gga.Hdop);
			Assert.AreEqual(1378, gga.Altitude);
			Assert.AreEqual("M", gga.AltitudeUnits);
			Assert.AreEqual(-22.1, gga.HeightOfGeoid);
			Assert.AreEqual("M", gga.HeightOfGeoidUnits);
			Assert.AreEqual(-1, gga.DgpsStationID);
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
