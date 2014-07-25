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
using System.Threading.Tasks;
using System.IO;

namespace NmeaParser.Tests
{
    [TestClass]
    public class NmeaMessages
    {
		[TestMethod]
		public async Task ParseNmeaFile()
		{
			var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///NmeaSampleData.txt"));
			System.IO.StreamReader reader = new System.IO.StreamReader(await file.OpenStreamForReadAsync());
			while(!reader.EndOfStream)
			{
				var line = reader.ReadLine();
				if(line.StartsWith("$"))
				{
					var msg = NmeaMessage.Parse(line);
					Assert.IsNotNull(msg);
					Assert.IsNotInstanceOfType(msg, typeof(Nmea.UnknownMessage), "Type " + msg.MessageType + " not supported");
				}
			}
		}

		[TestMethod]
		public void TestGprmb_Empty()
		{
			string input = "$GPRMB,A,,,,,,,,,,,,A,A*0B";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(Gprmb));
			Gprmb rmb = (Gprmb)msg;
			Assert.AreEqual(true, rmb.Arrived);
			Assert.AreEqual(double.NaN, rmb.CrossTrackError);
			Assert.AreEqual(double.NaN, rmb.DestinationLatitude);
			Assert.AreEqual(double.NaN, rmb.DestinationLongitude);
			Assert.AreEqual(0, rmb.DestinationWaypointID);
			Assert.AreEqual(0, rmb.OriginWaypointID);
			Assert.AreEqual(double.NaN, rmb.RangeToDestination);
			Assert.AreEqual(Gprmb.DataStatus.OK, rmb.Status);
			Assert.AreEqual(double.NaN, rmb.TrueBearing);
			Assert.AreEqual(double.NaN, rmb.Velocity);
		}

		[TestMethod]
		public void TestGprmb()
		{
			string input = "$GPRMB,A,0.66,L,003,004,4917.24,S,12309.57,W,001.3,052.5,000.5,V*3D";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(Gprmb));
			Gprmb rmb = (Gprmb)msg;
			Assert.AreEqual(Gprmb.DataStatus.OK, rmb.Status);
			Assert.AreEqual(-.66, rmb.CrossTrackError);
			Assert.AreEqual(3, rmb.OriginWaypointID);
			Assert.AreEqual(4, rmb.DestinationWaypointID);
			Assert.AreEqual(-49.287333333333333333, rmb.DestinationLatitude);
			Assert.AreEqual(-123.1595, rmb.DestinationLongitude);
			Assert.AreEqual(1.3, rmb.RangeToDestination);
			Assert.AreEqual(52.5, rmb.TrueBearing);
			Assert.AreEqual(.5, rmb.Velocity);
			Assert.AreEqual(false, rmb.Arrived);
		}

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

		[TestMethod]
		public void TestPgrme()
		{
			string input = "$PGRME,2.3,M,3.3,M,4.0,M*2B";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(NmeaParser.Nmea.Gps.Garmin.Pgrme));
			NmeaParser.Nmea.Gps.Garmin.Pgrme rme = (NmeaParser.Nmea.Gps.Garmin.Pgrme)msg;
			Assert.AreEqual(2.3, rme.HorizontalError);
			Assert.AreEqual("M", rme.HorizontalErrorUnits);
			Assert.AreEqual(3.3, rme.VerticalError);
			Assert.AreEqual("M", rme.VerticalErrorUnits);
			Assert.AreEqual(4.0, rme.SphericalError);
			Assert.AreEqual("M", rme.SphericalErrorUnits);			
		}
	}
}
