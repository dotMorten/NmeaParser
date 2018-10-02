//
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
using NmeaParser.Nmea;
using NmeaParser.Nmea.Gps;
using System.Threading.Tasks;
using System.IO;
using NmeaParser.Nmea.Gnss;
using NmeaParser.Nmea.Glonass;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NmeaParser.Tests
{
    [TestClass]
    public class NmeaMessages
    {
        [TestMethod]
        public async Task ParseNmeaFile()
        {
#if NETFX_CORE
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///NmeaSampleData.txt"));
            System.IO.StreamReader reader = new System.IO.StreamReader(await file.OpenStreamForReadAsync());
#else
            System.IO.StreamReader reader = new System.IO.StreamReader("NmeaSampleData.txt");
#endif
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line.StartsWith("$"))
                {
                    var msg = NmeaMessage.Parse(line);
                    Assert.IsNotNull(msg);
                    var idx = line.IndexOf('*');
                    if (idx >= 0)
                    {
                        byte checksum = (byte)Convert.ToInt32(line.Substring(idx + 1), 16);
                        Assert.AreEqual(checksum, msg.Checksum);
                    }
                    Assert.IsNotInstanceOfType(msg, typeof(Nmea.UnknownMessage), "Type " + msg.MessageType + " not supported");
                }
            }
        }
        [TestMethod]
        public async Task ParseTrimbleR2NmeaFile()
        {
#if NETFX_CORE
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///TrimbleR2SampleData.txt"));
            System.IO.StreamReader reader = new System.IO.StreamReader(await file.OpenStreamForReadAsync());
#else
            System.IO.StreamReader reader = new System.IO.StreamReader("TrimbleR2SampleData.txt");
#endif
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line.StartsWith("$"))
                {
                    var msg = NmeaMessage.Parse(line);
                    Assert.IsNotNull(msg);
                    var idx = line.IndexOf('*');
                    if (idx >= 0)
                    {
                        byte checksum = (byte)Convert.ToInt32(line.Substring(idx + 1), 16);
                        Assert.AreEqual(checksum, msg.Checksum);
                    }
                    if (msg.MessageType == "PTNL" || msg.MessageType == "GNVTG" || msg.MessageType == "GNZDA")
                        continue; //TODO
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
            Assert.AreEqual(0, rmb.DestinationWaypointId);
            Assert.AreEqual(0, rmb.OriginWaypointId);
            Assert.AreEqual(double.NaN, rmb.RangeToDestination);
            Assert.AreEqual(Gprmb.DataStatus.Ok, rmb.Status);
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
            Assert.AreEqual(Gprmb.DataStatus.Ok, rmb.Status);
            Assert.AreEqual(-.66, rmb.CrossTrackError);
            Assert.AreEqual(3, rmb.OriginWaypointId);
            Assert.AreEqual(4, rmb.DestinationWaypointId);
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
        public void TestGnrmc()
        {
            string input = "$GNRMC,231011.00,A,3403.47163804,N,11711.80926595,W,0.019,11.218,201217,12.0187,E,D*01";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gnrmc));
            Gnrmc rmc = (Gnrmc)msg;
            Assert.AreEqual("GNRMC", rmc.MessageType);
            Assert.AreEqual(new DateTime(2017, 12, 20, 23, 10, 11, DateTimeKind.Utc), rmc.FixTime);
            Assert.AreEqual(34.057860634, rmc.Latitude, 0.0000000001);
            Assert.AreEqual(-117.19682109916667, rmc.Longitude, 0.0000000001);
            Assert.AreEqual(true, rmc.Active);
            Assert.AreEqual(11.218, rmc.Course);
            Assert.AreEqual(12.0187, rmc.MagneticVariation);
            Assert.AreEqual(0.019, rmc.Speed);
        }

        [TestMethod]
        public void TestGpgga()
        {
            string input = "$GPGGA,235236,3925.9479,N,11945.9211,W,1,10,0.8,1378.0,M,-22.1,M,,*46";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gpgga));
            Gpgga gga = (Gpgga)msg;
            Assert.AreEqual(new TimeSpan(23, 52, 36), gga.FixTime);
            Assert.AreEqual(39.432465, gga.Latitude);
            Assert.AreEqual(-119.7653516666666667, gga.Longitude, 0.0000000001);
            Assert.AreEqual(NmeaParser.Nmea.Gps.Gpgga.FixQuality.GpsFix, gga.Quality);
            Assert.AreEqual(10, gga.NumberOfSatellites);
            Assert.AreEqual(.8, gga.Hdop);
            Assert.AreEqual(1378, gga.Altitude);
            Assert.AreEqual("M", gga.AltitudeUnits);
            Assert.AreEqual(-22.1, gga.HeightOfGeoid);
            Assert.AreEqual("M", gga.HeightOfGeoidUnits);
            Assert.AreEqual(-1, gga.DgpsStationId);
            Assert.AreEqual(TimeSpan.MaxValue, gga.TimeSinceLastDgpsUpdate);
        }

        [TestMethod]
        public void TestGngga()
        {
            string input = "$GNGGA,231011.00,3403.47163804,N,11711.80926595,W,5,13,0.9,403.641,M,-32.133,M,1.0,0000*6D";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gngga));
            Gngga gga = (Gngga)msg;
            Assert.AreEqual(new TimeSpan(23, 10, 11), gga.FixTime);
            Assert.AreEqual(34.057860634, gga.Latitude);
            Assert.AreEqual(-117.19682109916667, gga.Longitude, 0.0000000001);
            Assert.AreEqual(NmeaParser.Nmea.Gps.Gpgga.FixQuality.FloatRtk, gga.Quality);
            Assert.AreEqual(13, gga.NumberOfSatellites);
            Assert.AreEqual(.9, gga.Hdop);
            Assert.AreEqual(403.641, gga.Altitude);
            Assert.AreEqual("M", gga.AltitudeUnits);
            Assert.AreEqual(-32.133, gga.HeightOfGeoid);
            Assert.AreEqual("M", gga.HeightOfGeoidUnits);
            Assert.AreEqual(0, gga.DgpsStationId);
            Assert.AreEqual(TimeSpan.FromSeconds(1), gga.TimeSinceLastDgpsUpdate);
        }

        [TestMethod]
        public void TestPtlna()
        {
            string input = "$PTNLA,HV,002.94,M,288.1,D,008.6,D,002.98,M*74";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(NmeaParser.Nmea.LaserRange.Trimble.Ptnla));
            NmeaParser.Nmea.LaserRange.Trimble.Ptnla ptlna = (NmeaParser.Nmea.LaserRange.Trimble.Ptnla)msg;
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

        [TestMethod]
        public void TestGpgsa_Empty()
        {
            string input = "$GPGSA,A,3,,,,,,16,18,,22,24,,,,,*14";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gpgsa));
            Gpgsa gsa = (Gpgsa)msg;
            Assert.AreEqual(Gpgsa.ModeSelection.Auto, gsa.GpsMode);
            Assert.AreEqual(Gpgsa.Mode.Fix3D, gsa.FixMode);
            Assert.AreEqual(4, gsa.SVs.Count);
            Assert.AreEqual(16, gsa.SVs[0]);
            Assert.AreEqual(18, gsa.SVs[1]);
            Assert.AreEqual(22, gsa.SVs[2]);
            Assert.AreEqual(24, gsa.SVs[3]);
            Assert.AreEqual(double.NaN, gsa.Pdop);
            Assert.AreEqual(double.NaN, gsa.Hdop);
            Assert.AreEqual(double.NaN, gsa.Vdop);
        }

        [TestMethod]
        public void TestGpgsa()
        {
            string input = "$GPGSA,M,2,19,28,14,18,27,22,31,39,40,42,43,44,1.7,1.0,1.3*3C";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gpgsa));
            Gpgsa gsa = (Gpgsa)msg;
            Assert.AreEqual(Gpgsa.ModeSelection.Manual, gsa.GpsMode);
            Assert.AreEqual(Gpgsa.Mode.Fix2D, gsa.FixMode);
            Assert.AreEqual(12, gsa.SVs.Count);
            Assert.AreEqual(19, gsa.SVs[0]);
            Assert.AreEqual(28, gsa.SVs[1]);
            Assert.AreEqual(14, gsa.SVs[2]);
            Assert.AreEqual(18, gsa.SVs[3]);
            Assert.AreEqual(27, gsa.SVs[4]);
            Assert.AreEqual(22, gsa.SVs[5]);
            Assert.AreEqual(31, gsa.SVs[6]);
            Assert.AreEqual(39, gsa.SVs[7]);
            Assert.AreEqual(40, gsa.SVs[8]);
            Assert.AreEqual(42, gsa.SVs[9]);
            Assert.AreEqual(43, gsa.SVs[10]);
            Assert.AreEqual(44, gsa.SVs[11]);
            Assert.AreEqual(1.7, gsa.Pdop);
            Assert.AreEqual(1.0, gsa.Hdop);
            Assert.AreEqual(1.3, gsa.Vdop);
        }

        [TestMethod]
        public void TestGngsa()
        {
            string input = "$GNGSA,A,3,3,7,16,23,9,26,,,,,,,3.5,1.4,3.2*11";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gngsa));
            Gngsa gsa = (Gngsa)msg;
            Assert.AreEqual(Gpgsa.ModeSelection.Auto, gsa.GpsMode);
            Assert.AreEqual(Gpgsa.Mode.Fix3D, gsa.FixMode);
            Assert.AreEqual(6, gsa.SVs.Count);
            Assert.AreEqual(3, gsa.SVs[0]);
            Assert.AreEqual(7, gsa.SVs[1]);
            Assert.AreEqual(16, gsa.SVs[2]);
            Assert.AreEqual(23, gsa.SVs[3]);
            Assert.AreEqual(9, gsa.SVs[4]);
            Assert.AreEqual(26, gsa.SVs[5]);
            Assert.AreEqual(3.5, gsa.Pdop);
            Assert.AreEqual(1.4, gsa.Hdop);
            Assert.AreEqual(3.2, gsa.Vdop);
        }

        [TestMethod]
        public void TestGpgsv()
        {
            string input = "$GPGSV,3,3,11,22,42,067,42,75,14,311,43,50,05,244,00,,,,*49";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gpgsv));
            Gpgsv gsv = (Gpgsv)msg;
            Assert.AreEqual(3, gsv.TotalMessages);
            Assert.AreEqual(3, gsv.MessageNumber);
            Assert.AreEqual(11, gsv.SVsInView);
            Assert.IsNotNull(gsv.SVs);
            Assert.AreEqual(3, gsv.SVs.Count);
            var sv = gsv.SVs[0];
            Assert.AreEqual(22, sv.PrnNumber);
            Assert.AreEqual(42, sv.Elevation);
            Assert.AreEqual(67, sv.Azimuth);
            Assert.AreEqual(42, sv.SignalToNoiseRatio);
            Assert.AreEqual(SatelliteSystem.Gps, sv.System);

            sv = gsv.SVs[1];
            Assert.AreEqual(75, sv.PrnNumber);
            Assert.AreEqual(14, sv.Elevation);
            Assert.AreEqual(311, sv.Azimuth);
            Assert.AreEqual(43, sv.SignalToNoiseRatio);
            Assert.AreEqual(SatelliteSystem.Glonass, sv.System);

            sv = gsv.SVs[2];
            Assert.AreEqual(50, sv.PrnNumber);
            Assert.AreEqual(5, sv.Elevation);
            Assert.AreEqual(244, sv.Azimuth);
            Assert.AreEqual(00, sv.SignalToNoiseRatio);
            Assert.AreEqual(SatelliteSystem.Waas, sv.System);
        }

        [TestMethod]
        public void TestGpgsv_Empty()
        {
            string input = "$GPGSV,1,1,0,,,,,,,,,,,,,,,,*49";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gpgsv));
            Gpgsv gsv = (Gpgsv)msg;
            Assert.AreEqual(1, gsv.TotalMessages);
            Assert.AreEqual(1, gsv.MessageNumber);
            Assert.AreEqual(0, gsv.SVsInView);
            Assert.IsNotNull(gsv.SVs);
            Assert.AreEqual(0, gsv.SVs.Count);
        }

        [TestMethod]
        public void TestGpgll()
        {
            string input = "$GPGLL,4916.45,N,12311.12,W,225444.12,A,*30";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gpgll));
            Gpgll gll = (Gpgll)msg;
            Assert.IsTrue(gll.DataActive);
            Assert.AreEqual(49.2741666666666666667, gll.Latitude);
            Assert.AreEqual(-123.18533333333333333, gll.Longitude);
            Assert.AreEqual(new TimeSpan(0, 22, 54, 44, 120), gll.FixTime);
        }

        [TestMethod]
        public void TestGngll()
        {
            string input = "$GNGLL,3403.47121040,N,11711.80878910,W,235715.00,A,D*66";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gngll));
            Gngll gll = (Gngll)msg;
            Assert.IsTrue(gll.DataActive);
            Assert.AreEqual(34.0578535066667, gll.Latitude, .000000000001);
            Assert.AreEqual(-117.196813151667, gll.Longitude, .000000000001);
            Assert.AreEqual(new TimeSpan(0, 23, 57, 15, 0), gll.FixTime);
        }



        [TestMethod]
        public void TestGpgns()
        {
            string input = "$GPGNS,224749.00,3333.4268304,N,11153.3538273,W,D,19,0.6,406.110,-26.294,6.0,0138,S*6A";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gpgns));
            Gpgns gns = (Gpgns)msg;
            Assert.AreEqual(new TimeSpan(0, 22, 47, 49, 0), gns.FixTime);
            Assert.AreEqual(33.55711384, gns.Latitude, .000000000001);
            Assert.AreEqual(-111.889230455, gns.Longitude, .000000000001);
            Assert.AreEqual(Gns.Mode.Differential, gns.GpsModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.GlonassModeIndicator);
            Assert.AreEqual(0, gns.FutureModeIndicator.Length);
            Assert.AreEqual(19, gns.NumberOfSatellites);
            Assert.AreEqual(.6, gns.Hdop);
            Assert.AreEqual(406.110, gns.OrhometricHeight);
            Assert.AreEqual(-26.294, gns.GeoidalSeparation);
            Assert.AreEqual("0138", gns.DgpsStationId);
            Assert.AreEqual(Gns.NavigationalStatus.Safe, gns.Status);
            Assert.AreEqual(TimeSpan.FromSeconds(6), gns.TimeSinceLastDgpsUpdate);
        }

        [TestMethod]
        public void TestGpgns_NoData()
        {
            string input = "$GPGNS,235720.00,,,,,,6,,,,2.0,0*48";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gpgns));
            Gpgns gns = (Gpgns)msg;
            Assert.AreEqual(new TimeSpan(0, 23, 57, 20, 0), gns.FixTime);
            Assert.AreEqual(double.NaN, gns.Latitude);
            Assert.AreEqual(double.NaN, gns.Longitude);
            Assert.AreEqual(Gns.Mode.NoFix, gns.GpsModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.GlonassModeIndicator);
            Assert.AreEqual(0, gns.FutureModeIndicator.Length);
            Assert.AreEqual(6, gns.NumberOfSatellites);
            Assert.AreEqual(double.NaN, gns.Hdop);
            Assert.AreEqual(double.NaN, gns.OrhometricHeight);
            Assert.AreEqual(double.NaN, gns.GeoidalSeparation);
            Assert.AreEqual(TimeSpan.FromSeconds(2), gns.TimeSinceLastDgpsUpdate);
            Assert.AreEqual("0", gns.DgpsStationId);
        }

        [TestMethod]
        public void TestGngns()
        {
            string input = "$GNGNS,235719.00,3403.47068778,N,11711.80950154,W,DDNNN,10,1.4,402.411,-32.133,,*26";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gngns));
            Gngns gns = (Gngns)msg;
            Assert.AreEqual(new TimeSpan(0, 23, 57, 19, 0), gns.FixTime);
            Assert.AreEqual(34.0578447963333, gns.Latitude, .000000000001);
            Assert.AreEqual(-117.196825025667, gns.Longitude, .00000000001);
            Assert.AreEqual(Gns.Mode.Differential, gns.GpsModeIndicator);
            Assert.AreEqual(Gns.Mode.Differential, gns.GlonassModeIndicator);
            Assert.AreEqual(3, gns.FutureModeIndicator.Length);
            Assert.AreEqual(Gns.Mode.NoFix, gns.FutureModeIndicator[0]);
            Assert.AreEqual(Gns.Mode.NoFix, gns.FutureModeIndicator[1]);
            Assert.AreEqual(Gns.Mode.NoFix, gns.FutureModeIndicator[2]);
            Assert.AreEqual(10, gns.NumberOfSatellites);
            Assert.AreEqual(1.4, gns.Hdop);
            Assert.AreEqual(402.411, gns.OrhometricHeight);
            Assert.AreEqual(-32.133, gns.GeoidalSeparation);
            Assert.AreEqual(TimeSpan.MaxValue, gns.TimeSinceLastDgpsUpdate);
            Assert.AreEqual(null, gns.DgpsStationId);
        }


        [TestMethod]
        public void TestGlgns()
        {
            string input = "$GLGNS,235720.00,,,,,,4,,,,2.0,0*56";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Glgns));
            Glgns gns = (Glgns)msg;
            Assert.AreEqual(new TimeSpan(0, 23, 57, 20, 0), gns.FixTime);
            Assert.AreEqual(double.NaN, gns.Latitude);
            Assert.AreEqual(double.NaN, gns.Longitude);
            Assert.AreEqual(Gns.Mode.NoFix, gns.GpsModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.GlonassModeIndicator);
            Assert.AreEqual(0, gns.FutureModeIndicator.Length);
            Assert.AreEqual(4, gns.NumberOfSatellites);
            Assert.AreEqual(double.NaN, gns.Hdop);
            Assert.AreEqual(double.NaN, gns.OrhometricHeight);
            Assert.AreEqual(double.NaN, gns.GeoidalSeparation);
            Assert.AreEqual(TimeSpan.FromSeconds(2), gns.TimeSinceLastDgpsUpdate);
        }

        [TestMethod]
		public void TestGpgll_NoFixTime_OrActiveIndicator()
		{
			string input = "$GPGLL,3751.65,S,14507.36,E*77";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(Gpgll));
			Gpgll gll = (Gpgll)msg;
			Assert.IsTrue(gll.DataActive);
			Assert.AreEqual(-37.860833333333333333, gll.Latitude);
			Assert.AreEqual(145.1226666666666666667, gll.Longitude);
			Assert.AreEqual(TimeSpan.Zero, gll.FixTime);
		}


		[TestMethod]
		public void TestGpbod_Empty()
		{
			string input = "$GPBOD,,T,,M,,*47";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(Gpbod));
			Gpbod bod = (Gpbod)msg;
			Assert.AreEqual(double.NaN, bod.TrueBearing, "TrueBearing");
			Assert.AreEqual(double.NaN, bod.MagneticBearing, "MagneticBearing");
			Assert.IsNull(bod.OriginId, "OriginID");
			Assert.IsNull(bod.DestinationId, "DestinationID");
		}

		[TestMethod]
		public void TestGpbod_GoToMode()
		{
			string input = "$GPBOD,099.3,T,105.6,M,POINTB,*48";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(Gpbod));
			Gpbod bod = (Gpbod)msg;
			Assert.AreEqual(99.3, bod.TrueBearing, "TrueBearing");
			Assert.AreEqual(105.6, bod.MagneticBearing, "MagneticBearing");
			Assert.AreEqual("POINTB", bod.DestinationId, "DestinationID");
			Assert.IsNull(bod.OriginId, "OriginID");
		}


		[TestMethod]
		public void TestGpbod()
		{
			string input = "$GPBOD,097.0,T,103.2,M,POINTB,POINTA*4A";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(Gpbod));
			Gpbod bod = (Gpbod)msg;
			Assert.AreEqual(97d, bod.TrueBearing, "TrueBearing");
			Assert.AreEqual(103.2, bod.MagneticBearing, "MagneticBearing");
			Assert.AreEqual("POINTB", bod.DestinationId, "DestinationID");
			Assert.AreEqual("POINTA", bod.OriginId, "OriginID");
		}


		[TestMethod]
		public void TestPgrmz_Empty()
		{
			string input = "$PGRMZ,,,*7E";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(NmeaParser.Nmea.Gps.Garmin.Pgrmz));
			var rmz = (NmeaParser.Nmea.Gps.Garmin.Pgrmz)msg;
			Assert.AreEqual(double.NaN, rmz.Altitude, "Altitude");
			Assert.AreEqual(NmeaParser.Nmea.Gps.Garmin.Pgrmz.AltitudeUnit.Unknown, rmz.Unit, "Unit");
			Assert.AreEqual(NmeaParser.Nmea.Gps.Garmin.Pgrmz.PositionFixType.Unknown, rmz.FixType, "FixDimension");
		}

		[TestMethod]
		public void TestPgrmz()
		{
			string input = "$PGRMZ,93,f,3*21";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(NmeaParser.Nmea.Gps.Garmin.Pgrmz));
			var rmz = (NmeaParser.Nmea.Gps.Garmin.Pgrmz)msg;
			Assert.AreEqual(93d, rmz.Altitude, "Altitude");
			Assert.AreEqual(NmeaParser.Nmea.Gps.Garmin.Pgrmz.AltitudeUnit.Feet, rmz.Unit, "Unit");
			Assert.AreEqual(NmeaParser.Nmea.Gps.Garmin.Pgrmz.PositionFixType.Fix3D, rmz.FixType, "FixDimension");
		}

		[TestMethod]
		public void TestGprte()
		{
			string input = "$GPRTE,2,1,c,0,W3IWI,DRIVWY,32CEDR,32-29,32BKLD,32-I95,32-US1,BW-32,BW-198*69";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(Gprte));
			Gprte gsv = (Gprte)msg;
			Assert.AreEqual(2, gsv.TotalMessages);
			Assert.AreEqual(1, gsv.MessageNumber);
			Assert.AreEqual(NmeaParser.Nmea.Gps.Gprte.WaypointListType.CompleteWaypointsList, gsv.ListType);
			Assert.AreEqual("0", gsv.RouteId);
			Assert.AreEqual("0", gsv.RouteId);
			Assert.AreEqual(9, gsv.Waypoints.Count);
			Assert.AreEqual("W3IWI", gsv.Waypoints[0]);
			Assert.AreEqual("32BKLD", gsv.Waypoints[4]);
			Assert.AreEqual("BW-198", gsv.Waypoints[8]);
		}

		[TestMethod]
		public void TestGpgst()
		{
			string input = "$GPGST,172814.0,0.006,0.023,0.020,273.6,0.023,0.020,0.031*6A";
			var msg = NmeaMessage.Parse(input);
			Assert.IsInstanceOfType(msg, typeof(Gpgst));
			Gpgst gst = (Gpgst)msg;
			Assert.AreEqual(new TimeSpan(17, 28, 14), gst.FixTime);
			Assert.AreEqual(0.006, gst.Rms);
			Assert.AreEqual(0.023, gst.SemiMajorError);
			Assert.AreEqual(0.02, gst.SemiMinorError);
			Assert.AreEqual(273.6, gst.ErrorOrientation);
			Assert.AreEqual(0.023, gst.SigmaLatitudeError);
			Assert.AreEqual(0.020, gst.SigmaLongitudeError);
			Assert.AreEqual(0.031, gst.SigmaHeightError);
		}

        [TestMethod]
        public void TestGngst()
        {
            string input = "$GNGST,172814.0,0.006,0.023,0.020,273.6,0.023,0.020,0.031*74";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gngst));
            Gngst gst = (Gngst)msg;
            Assert.AreEqual(new TimeSpan(17, 28, 14), gst.FixTime);
            Assert.AreEqual(0.006, gst.Rms);
            Assert.AreEqual(0.023, gst.SemiMajorError);
            Assert.AreEqual(0.02, gst.SemiMinorError);
            Assert.AreEqual(273.6, gst.ErrorOrientation);
            Assert.AreEqual(0.023, gst.SigmaLatitudeError);
            Assert.AreEqual(0.020, gst.SigmaLongitudeError);
            Assert.AreEqual(0.031, gst.SigmaHeightError);
        }

        [TestMethod]
        public void TestGpvtg()
        {
            string input = "$GPVTG,103.85,T,92.79,M,0.14,N,0.25,K,D*1E";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gpvtg));
            Gpvtg vtg = (Gpvtg)msg;
            Assert.AreEqual(103.85, vtg.TrueCourseOverGround);
            Assert.AreEqual(92.79, vtg.MagneticCourseOverGround);
            Assert.AreEqual(0.14, vtg.SpeedInKnots);
            Assert.AreEqual(0.25, vtg.SpeedInKph);
        }

        [TestMethod]
        public void TestGpvtg_Empty()
        {
            string input = "$GPVTG,,T,,M,0.00,N,0.00,K*4E";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gpvtg));
            Gpvtg vtg = (Gpvtg)msg;
            Assert.IsTrue(double.IsNaN(vtg.TrueCourseOverGround));
            Assert.IsTrue(double.IsNaN(vtg.MagneticCourseOverGround));
            Assert.AreEqual(0.0, vtg.SpeedInKnots);
            Assert.AreEqual(0.0, vtg.SpeedInKph);
        }

        [TestMethod]
        public void TestGnzda()
        {
            var input = "$GNZDA,075451.00,02,10,2018,00,00*72";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gnzda));
            var zda = (Gnzda)msg;
            Assert.AreEqual(new DateTime(2018, 10, 02, 07, 54, 51, 00, DateTimeKind.Utc), zda.FixDateTime);
        }

        [TestMethod]
        public void TestGpzda()
        {
            var input = "$GPZDA,143042.00,25,08,2005,,*6E";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gpzda));
            var zda = (Gpzda)msg;
            Assert.AreEqual(new DateTime(2005, 08, 25, 14, 30, 42, 00, DateTimeKind.Utc), zda.FixDateTime);
        }

        [TestMethod]
        public void TestGlzda()
        {
            var input = "$GLZDA,225627.00,21,09,2015,00,00*70";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Glzda));
            var zda = (Glzda)msg;
            Assert.AreEqual(new DateTime(2015, 09, 21, 22, 56, 27, 00, DateTimeKind.Utc), zda.FixDateTime);
        }
    }
}
