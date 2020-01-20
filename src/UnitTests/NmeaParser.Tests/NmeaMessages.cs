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
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NmeaParser.Tests
{
    [TestClass]
    public class NmeaMessages
    {
        [TestMethod]
        public
#if NETFX_CORE
            async Task
#else
            void
#endif
            ParseNmeaFile()
        {
#if NETFX_CORE
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///NmeaSampleData.txt"));
            System.IO.StreamReader reader = new System.IO.StreamReader(await file.OpenStreamForReadAsync());
#else
            System.IO.StreamReader reader = new System.IO.StreamReader("NmeaSampleData.txt");
#endif
            NmeaMessage previousMessage = null;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line.StartsWith("$"))
                {
                    var msg = NmeaMessage.Parse(line, previousMessage as IMultiSentenceMessage);
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
        public
#if NETFX_CORE
            async Task
#else
            void
#endif
            ParseTrimbleR2NmeaFile()
        {
#if NETFX_CORE
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///TrimbleR2SampleData.txt"));
            System.IO.StreamReader reader = new System.IO.StreamReader(await file.OpenStreamForReadAsync());
#else
            System.IO.StreamReader reader = new System.IO.StreamReader("TrimbleR2SampleData.txt");
#endif
            NmeaMessage previousMessage = null;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line.StartsWith("$"))
                {
                    var msg = NmeaMessage.Parse(line, previousMessage as IMultiSentenceMessage);
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
        public void TestGprma()
        {
            string input = "$GPRMA,A,4917.24,S,12309.57,W,1000.0,2000.0,123.4,321.0,10,E,A*38";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Rma));
            Rma rma = (Rma)msg;
            Assert.AreEqual(Rma.PositioningStatus.Autonomous, rma.Status);
            Assert.AreEqual(-49.287333333333333333, rma.Latitude);
            Assert.AreEqual(-123.1595, rma.Longitude);
            Assert.AreEqual(TimeSpan.FromMilliseconds(1), rma.TimeDifferenceA);
            Assert.AreEqual(TimeSpan.FromMilliseconds(2), rma.TimeDifferenceB);
            Assert.AreEqual(123.4, rma.Speed);
            Assert.AreEqual(321, rma.Course);
            Assert.AreEqual(-10, rma.MagneticVariation);
            Assert.AreEqual(Rma.PositioningMode.Autonomous, rma.Mode);
        }

        [TestMethod]
        public void TestGprmb_Empty()
        {
            string input = "$GPRMB,A,,,,,,,,,,,,A,A*0B";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Rmb));
            Rmb rmb = (Rmb)msg;
            Assert.AreEqual(true, rmb.Arrived);
            Assert.AreEqual(double.NaN, rmb.CrossTrackError);
            Assert.AreEqual(double.NaN, rmb.DestinationLatitude);
            Assert.AreEqual(double.NaN, rmb.DestinationLongitude);
            Assert.AreEqual(0, rmb.DestinationWaypointId);
            Assert.AreEqual(0, rmb.OriginWaypointId);
            Assert.AreEqual(double.NaN, rmb.RangeToDestination);
            Assert.AreEqual(Rmb.DataStatus.Ok, rmb.Status);
            Assert.AreEqual(double.NaN, rmb.TrueBearing);
            Assert.AreEqual(double.NaN, rmb.Velocity);
        }

        [TestMethod]
        public void TestGprmb()
        {
            string input = "$GPRMB,A,0.66,L,003,004,4917.24,S,12309.57,W,001.3,052.5,000.5,V*3D";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Rmb));
            Rmb rmb = (Rmb)msg;
            Assert.AreEqual(Rmb.DataStatus.Ok, rmb.Status);
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
            Assert.IsInstanceOfType(msg, typeof(Rmc));
            Rmc rmc = (Rmc)msg;
            Assert.AreEqual(new DateTimeOffset(2013, 03, 23, 12, 35, 19, TimeSpan.Zero), rmc.FixTime);
            Assert.AreEqual(-48.1173, rmc.Latitude);
            Assert.AreEqual(-11.516666666666667, rmc.Longitude, 0.0000000001);
        }

        [TestMethod]
        public void TestGnrmc()
        {
            string input = "$GNRMC,231011.00,A,3403.47163804,N,11711.80926595,W,0.019,11.218,201217,12.0187,E,D*01";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Rmc));
            Rmc rmc = (Rmc)msg;
            Assert.AreEqual("GNRMC", rmc.MessageType);
            Assert.AreEqual(new DateTimeOffset(2017, 12, 20, 23, 10, 11, TimeSpan.Zero), rmc.FixTime);
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
            Assert.IsInstanceOfType(msg, typeof(Gga));
            Gga gga = (Gga)msg;
            Assert.AreEqual(new TimeSpan(23, 52, 36), gga.FixTime);
            Assert.AreEqual(39.432465, gga.Latitude);
            Assert.AreEqual(-119.7653516666666667, gga.Longitude, 0.0000000001);
            Assert.AreEqual(NmeaParser.Nmea.Gga.FixQuality.GpsFix, gga.Quality);
            Assert.AreEqual(10, gga.NumberOfSatellites);
            Assert.AreEqual(.8, gga.Hdop);
            Assert.AreEqual(1378, gga.Altitude);
            Assert.AreEqual("M", gga.AltitudeUnits);
            Assert.AreEqual(-22.1, gga.GeoidalSeparation);
            Assert.AreEqual("M", gga.GeoidalSeparationUnits);
            Assert.AreEqual(-1, gga.DgpsStationId);
            Assert.AreEqual(TimeSpan.MaxValue, gga.TimeSinceLastDgpsUpdate);
        }

        [TestMethod]
        public void TestGngga()
        {
            string input = "$GNGGA,231011.00,3403.47163804,N,11711.80926595,W,5,13,0.9,403.641,M,-32.133,M,1.0,0000*6D";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gga));
            Gga gga = (Gga)msg;
            Assert.AreEqual(new TimeSpan(23, 10, 11), gga.FixTime);
            Assert.AreEqual(34.057860634, gga.Latitude);
            Assert.AreEqual(-117.19682109916667, gga.Longitude, 0.0000000001);
            Assert.AreEqual(NmeaParser.Nmea.Gga.FixQuality.FloatRtk, gga.Quality);
            Assert.AreEqual(13, gga.NumberOfSatellites);
            Assert.AreEqual(.9, gga.Hdop);
            Assert.AreEqual(403.641, gga.Altitude);
            Assert.AreEqual("M", gga.AltitudeUnits);
            Assert.AreEqual(-32.133, gga.GeoidalSeparation);
            Assert.AreEqual("M", gga.GeoidalSeparationUnits);
            Assert.AreEqual(0, gga.DgpsStationId);
            Assert.AreEqual(TimeSpan.FromSeconds(1), gga.TimeSinceLastDgpsUpdate);
        }

        [TestMethod]
        public void TestPtlna()
        {
            string input = "$PTNLA,HV,002.94,M,288.1,D,008.6,D,002.98,M*74";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(NmeaParser.Nmea.Trimble.Ptnla));
            Assert.AreEqual(Talker.ProprietaryCode, msg.TalkerId);
            NmeaParser.Nmea.Trimble.Ptnla ptlna = (NmeaParser.Nmea.Trimble.Ptnla)msg;
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
            Assert.IsInstanceOfType(msg, typeof(NmeaParser.Nmea.Garmin.Pgrme));
            Assert.AreEqual(Talker.ProprietaryCode, msg.TalkerId);
            NmeaParser.Nmea.Garmin.Pgrme rme = (NmeaParser.Nmea.Garmin.Pgrme)msg;
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
            Assert.IsInstanceOfType(msg, typeof(Gsa));
            Gsa gsa = (Gsa)msg;
            Assert.AreEqual(Gsa.ModeSelection.Auto, gsa.Mode);
            Assert.AreEqual(Gsa.FixType.Fix3D, gsa.Fix);
            Assert.AreEqual(4, gsa.SatelliteIDs.Length);
            Assert.AreEqual(16, gsa.SatelliteIDs[0]);
            Assert.AreEqual(18, gsa.SatelliteIDs[1]);
            Assert.AreEqual(22, gsa.SatelliteIDs[2]);
            Assert.AreEqual(24, gsa.SatelliteIDs[3]);
            Assert.AreEqual(double.NaN, gsa.Pdop);
            Assert.AreEqual(double.NaN, gsa.Hdop);
            Assert.AreEqual(double.NaN, gsa.Vdop);
        }

        [TestMethod]
        public void TestGpgsa()
        {
            string input = "$GPGSA,M,2,19,28,14,18,27,22,31,39,40,42,43,44,1.7,1.0,1.3*3C";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gsa));
            Gsa gsa = (Gsa)msg;
            Assert.AreEqual(Gsa.ModeSelection.Manual, gsa.Mode);
            Assert.AreEqual(Gsa.FixType.Fix2D, gsa.Fix);
            Assert.AreEqual(12, gsa.SatelliteIDs.Length);
            Assert.AreEqual(19, gsa.SatelliteIDs[0]);
            Assert.AreEqual(28, gsa.SatelliteIDs[1]);
            Assert.AreEqual(14, gsa.SatelliteIDs[2]);
            Assert.AreEqual(18, gsa.SatelliteIDs[3]);
            Assert.AreEqual(27, gsa.SatelliteIDs[4]);
            Assert.AreEqual(22, gsa.SatelliteIDs[5]);
            Assert.AreEqual(31, gsa.SatelliteIDs[6]);
            Assert.AreEqual(39, gsa.SatelliteIDs[7]);
            Assert.AreEqual(40, gsa.SatelliteIDs[8]);
            Assert.AreEqual(42, gsa.SatelliteIDs[9]);
            Assert.AreEqual(43, gsa.SatelliteIDs[10]);
            Assert.AreEqual(44, gsa.SatelliteIDs[11]);
            Assert.AreEqual(1.7, gsa.Pdop);
            Assert.AreEqual(1.0, gsa.Hdop);
            Assert.AreEqual(1.3, gsa.Vdop);
        }

        [TestMethod]
        public void TestGngsa()
        {
            string input = "$GNGSA,A,3,3,7,16,23,9,26,,,,,,,3.5,1.4,3.2*11";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gsa));
            Assert.AreEqual("GNGSA", msg.MessageType);
            Gsa gsa = (Gsa)msg;
            Assert.AreEqual(Gsa.ModeSelection.Auto, gsa.Mode);
            Assert.AreEqual(Gsa.FixType.Fix3D, gsa.Fix);
            Assert.AreEqual(6, gsa.SatelliteIDs.Length);
            Assert.AreEqual(3, gsa.SatelliteIDs[0]);
            Assert.AreEqual(7, gsa.SatelliteIDs[1]);
            Assert.AreEqual(16, gsa.SatelliteIDs[2]);
            Assert.AreEqual(23, gsa.SatelliteIDs[3]);
            Assert.AreEqual(9, gsa.SatelliteIDs[4]);
            Assert.AreEqual(26, gsa.SatelliteIDs[5]);
            Assert.AreEqual(3.5, gsa.Pdop);
            Assert.AreEqual(1.4, gsa.Hdop);
            Assert.AreEqual(3.2, gsa.Vdop);
        }

        [TestMethod]
        public void TestGpgsv()
        {
            string input = "$GPGSV,3,3,11,22,42,067,42,75,14,311,43,50,05,244,00,,,,*49";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gsv));
            Gsv gsv = (Gsv)msg;
            Assert.IsInstanceOfType(msg, typeof(IMultiSentenceMessage));
            Assert.IsFalse(((IMultiSentenceMessage)msg).IsComplete);
            Assert.AreEqual(11, gsv.SatellitesInView);
            Assert.IsNotNull(gsv.SVs);
            Assert.AreEqual(3, gsv.SVs.Count);
            var sv = gsv.SVs[0];
            Assert.AreEqual(22, sv.Id);
            Assert.AreEqual(42, sv.Elevation);
            Assert.AreEqual(67, sv.Azimuth);
            Assert.AreEqual(42, sv.SignalToNoiseRatio);
            Assert.AreEqual(SatelliteSystem.Gps, sv.System);

            sv = gsv.SVs[1];
            Assert.AreEqual(75, sv.Id);
            Assert.AreEqual(14, sv.Elevation);
            Assert.AreEqual(311, sv.Azimuth);
            Assert.AreEqual(43, sv.SignalToNoiseRatio);
            Assert.AreEqual(SatelliteSystem.Glonass, sv.System);

            sv = gsv.SVs[2];
            Assert.AreEqual(50, sv.Id);
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
            Assert.IsInstanceOfType(msg, typeof(Gsv));
            Gsv gsv = (Gsv)msg;
            Assert.IsTrue(((IMultiSentenceMessage)gsv).IsComplete);
            Assert.AreEqual(0, gsv.SatellitesInView);
            Assert.IsNotNull(gsv.SVs);
            Assert.AreEqual(0, gsv.SVs.Count);
        }

        [TestMethod]
        public void TestGpgsv_Multi()
        {
            var input1 = "$GPGSV,3,1,9,00,30,055,48,00,19,281,00,27,19,275,00,12,16,319,00*4C";
            var input2 = "$GPGSV,3,2,9,00,30,055,48,00,19,281,00,27,19,275,00,12,16,319,00*4F";
            var input3 = "$GPGSV,3,3,9,32,10,037,00,,,,,,,,,,,,*74";
            var msg1 = NmeaMessage.Parse(input1);
            Assert.IsFalse(((IMultiSentenceMessage)msg1).IsComplete);
            var msg2 = NmeaMessage.Parse(input2, msg1 as IMultiSentenceMessage);
            Assert.IsFalse(((IMultiSentenceMessage)msg2).IsComplete);
            var msg3 = NmeaMessage.Parse(input3, msg2 as IMultiSentenceMessage);
            Assert.IsTrue(((IMultiSentenceMessage)msg3).IsComplete);
            Assert.IsInstanceOfType(msg1, typeof(Gsv));
            Assert.AreSame(msg1, msg2);
            Assert.AreSame(msg1, msg3);
            Gsv gsv = (Gsv)msg1;
            Assert.AreEqual(9, gsv.SatellitesInView);
            Assert.IsNotNull(gsv.SVs);
            Assert.AreEqual(9, gsv.SVs.Count);
        }

        [TestMethod]
        public void TestGpgsv_MultiMissing()
        {
            var input1 = "$GPGSV,2,1,9,00,30,055,48,00,19,281,00,27,19,275,00,12,16,319,00*4D";
            var input2 = "$GPGSV,2,2,8,00,30,055,48,00,19,281,00,27,19,275,00,12,16,319,00*4F"; //Satellite count doesn't match, so append will fail
            var msg1 = NmeaMessage.Parse(input1);
            Assert.IsFalse(((IMultiSentenceMessage)msg1).IsComplete);
            var msg2 = NmeaMessage.Parse(input2, msg1 as IMultiSentenceMessage);
            Assert.IsFalse(((IMultiSentenceMessage)msg2).IsComplete);
            Assert.IsInstanceOfType(msg2, typeof(Gsv));
            Assert.AreNotSame(msg1, msg2);
            Gsv gsv1 = (Gsv)msg1;
            Assert.AreEqual(9, gsv1.SatellitesInView);
            Assert.IsNotNull(gsv1.SVs);
            Assert.AreEqual(4, gsv1.SVs.Count);
            Gsv gsv2 = (Gsv)msg2;
            Assert.AreEqual(8, gsv2.SatellitesInView);
            Assert.IsNotNull(gsv2.SVs);
            Assert.AreEqual(4, gsv2.SVs.Count);
        }


        [TestMethod]
        public void TestGpgsv_MultiNotMatching()
        {
            var input2 = "$GPGSV,3,2,9,00,30,055,48,00,19,281,00,27,19,275,00,12,16,319,00*4F";
            var input3 = "$GPGSV,3,3,9,32,10,037,00,,,,,,,,,,,,*74";
            var msg2 = NmeaMessage.Parse(input2);
            Assert.IsFalse(((IMultiSentenceMessage)msg2).IsComplete);
            var msg3 = NmeaMessage.Parse(input3, msg2 as IMultiSentenceMessage);
            Assert.IsFalse(((IMultiSentenceMessage)msg3).IsComplete);
            Assert.IsInstanceOfType(msg2, typeof(Gsv));
            Assert.AreNotSame(msg2, msg3);
            Gsv gsv2 = (Gsv)msg2;
            Assert.AreEqual(9, gsv2.SatellitesInView);
            Assert.IsNotNull(gsv2.SVs);
            Assert.AreEqual(4, gsv2.SVs.Count);
            Gsv gsv3 = (Gsv)msg3;
            Assert.AreEqual(9, gsv3.SatellitesInView);
            Assert.IsNotNull(gsv3.SVs);
            Assert.AreEqual(1, gsv3.SVs.Count);
        }

        [TestMethod]
        [WorkItem(53)]
        public void TestGpgsv_MissingElevationAndAzimuth()
        {
            string msgstr = "$GPGSV,3,1,12,02,06,225,16,04,,,40,05,65,251,27,07,40,057,43,0*51";
            var msg = NmeaMessage.Parse(msgstr);
            Assert.IsInstanceOfType(msg, typeof(Gsv));
            Gsv gsv = (Gsv)msg;
            Assert.IsFalse(((IMultiSentenceMessage)gsv).IsComplete);
            Assert.AreEqual(12, gsv.SatellitesInView);
            Assert.IsNotNull(gsv.SVs);
            Assert.AreEqual(4, gsv.SVs.Count);
            Assert.AreEqual(4, gsv.SVs[1].Id);
            Assert.IsTrue(double.IsNaN(gsv.SVs[1].Elevation));
            Assert.IsTrue(double.IsNaN(gsv.SVs[1].Azimuth));
            Assert.AreEqual(40, gsv.SVs[1].SignalToNoiseRatio);
        }

        [TestMethod]
        public void TestGpgll()
        {
            string input = "$GPGLL,4916.45,N,12311.12,W,225444.12,A,*30";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gll));
            Gll gll = (Gll)msg;
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
            Assert.IsInstanceOfType(msg, typeof(Gll));
            Gll gll = (Gll)msg;
            Assert.IsTrue(gll.DataActive);
            Assert.AreEqual(34.0578535066667, gll.Latitude, .000000000001);
            Assert.AreEqual(-117.196813151667, gll.Longitude, .000000000001);
            Assert.AreEqual(new TimeSpan(0, 23, 57, 15, 0), gll.FixTime);
        }
        [TestMethod]
        public void TestLcgll()
        {
            string input = "$LCGLL,4728.31,N,12254.25,W,091342,A,A*4C";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gll));
            Gll gll = (Gll)msg;
            Assert.AreEqual(Talker.LoranC, gll.TalkerId);
            Assert.IsTrue(gll.DataActive);
            Assert.AreEqual(47.471833333333336, gll.Latitude);
            Assert.AreEqual(-122.90416666666667, gll.Longitude);
            Assert.AreEqual(new TimeSpan(0, 9, 13, 42, 0), gll.FixTime);
            Assert.AreEqual(Gll.Mode.Autonomous, gll.ModeIndicator);
        }



        [TestMethod]
        public void TestGpgns()
        {
            string input = "$GPGNS,224749.00,3333.4268304,N,11153.3538273,W,D,19,0.6,406.110,-26.294,6.0,0138,S*6A";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gns));
            Gns gns = (Gns)msg;
            Assert.AreEqual(new TimeSpan(0, 22, 47, 49, 0), gns.FixTime);
            Assert.AreEqual(33.55711384, gns.Latitude, .000000000001);
            Assert.AreEqual(-111.889230455, gns.Longitude, .000000000001);
            Assert.AreEqual(Gns.Mode.Differential, gns.GpsModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.GlonassModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.GalileoModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.BDSModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.QZSSModeIndicator);
            Assert.AreEqual(1, gns.ModeIndicators.Length);
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
            Assert.IsInstanceOfType(msg, typeof(Gns));
            Gns gns = (Gns)msg;
            Assert.AreEqual(Talker.GlobalPositioningSystem, gns.TalkerId);
            Assert.AreEqual(new TimeSpan(0, 23, 57, 20, 0), gns.FixTime);
            Assert.AreEqual(double.NaN, gns.Latitude);
            Assert.AreEqual(double.NaN, gns.Longitude);
            Assert.AreEqual(Gns.Mode.NoFix, gns.GpsModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.GlonassModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.GalileoModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.BDSModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.QZSSModeIndicator);
            Assert.AreEqual(0, gns.ModeIndicators.Length);
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
            Assert.IsInstanceOfType(msg, typeof(Gns));
            Gns gns = (Gns)msg;
            Assert.AreEqual(Talker.GlobalNavigationSatelliteSystem, gns.TalkerId);
            Assert.AreEqual(new TimeSpan(0, 23, 57, 19, 0), gns.FixTime);
            Assert.AreEqual(34.0578447963333, gns.Latitude, .000000000001);
            Assert.AreEqual(-117.196825025667, gns.Longitude, .00000000001);
            Assert.AreEqual(Gns.Mode.Differential, gns.GpsModeIndicator);
            Assert.AreEqual(Gns.Mode.Differential, gns.GlonassModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.GalileoModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.BDSModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.QZSSModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.NavICModeIndicator);
            Assert.AreEqual(5, gns.ModeIndicators.Length);
            Assert.AreEqual(Gns.Mode.Differential, gns.ModeIndicators[0]);
            Assert.AreEqual(Gns.Mode.Differential, gns.ModeIndicators[1]);
            Assert.AreEqual(Gns.Mode.NoFix, gns.ModeIndicators[2]);
            Assert.AreEqual(Gns.Mode.NoFix, gns.ModeIndicators[3]);
            Assert.AreEqual(Gns.Mode.NoFix, gns.ModeIndicators[4]);
            Assert.AreEqual(10, gns.NumberOfSatellites);
            Assert.AreEqual(1.4, gns.Hdop);
            Assert.AreEqual(402.411, gns.OrhometricHeight);
            Assert.AreEqual(-32.133, gns.GeoidalSeparation);
            Assert.AreEqual(TimeSpan.MaxValue, gns.TimeSinceLastDgpsUpdate);
            Assert.AreEqual(null, gns.DgpsStationId);
            Assert.AreEqual(Gns.NavigationalStatus.NotValid, gns.Status);
        }

        [TestMethod]
        public void TestGngnsModeIndicators()
        {
            string input = "$GNGNS,122310.2,3722.425671,N,12258.856215,W,DAAAAA,14,0.9,1005.543,6.5,,,S*0E";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gns));
            Gns gns = (Gns)msg;
            Assert.AreEqual(Gns.Mode.Differential, gns.GpsModeIndicator);
            Assert.AreEqual(Gns.Mode.Autonomous, gns.GlonassModeIndicator);
            Assert.AreEqual(Gns.Mode.Autonomous, gns.GalileoModeIndicator);
            Assert.AreEqual(Gns.Mode.Autonomous, gns.BDSModeIndicator);
            Assert.AreEqual(Gns.Mode.Autonomous, gns.QZSSModeIndicator);
            Assert.AreEqual(Gns.Mode.Autonomous, gns.NavICModeIndicator);
            Assert.AreEqual(6, gns.ModeIndicators.Length);
            Assert.AreEqual(Gns.Mode.Differential, gns.ModeIndicators[0]);
            Assert.AreEqual(Gns.Mode.Autonomous, gns.ModeIndicators[1]);
            Assert.AreEqual(Gns.Mode.Autonomous, gns.ModeIndicators[2]);
            Assert.AreEqual(Gns.Mode.Autonomous, gns.ModeIndicators[3]);
            Assert.AreEqual(Gns.Mode.Autonomous, gns.ModeIndicators[4]);
            Assert.AreEqual(Gns.Mode.Autonomous, gns.ModeIndicators[5]);
            Assert.AreEqual(null, gns.DgpsStationId);
            Assert.AreEqual(Gns.NavigationalStatus.Safe, gns.Status);
        }


        [TestMethod]
        public void TestGlgns()
        {
            string input = "$GLGNS,235720.00,,,,,,4,,,,2.0,0*56";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gns));
            Gns gns = (Gns)msg;
            Assert.AreEqual(new TimeSpan(0, 23, 57, 20, 0), gns.FixTime);
            Assert.AreEqual(Talker.GlonassReceiver, gns.TalkerId);
            Assert.AreEqual(double.NaN, gns.Latitude);
            Assert.AreEqual(double.NaN, gns.Longitude);
            Assert.AreEqual(Gns.Mode.NoFix, gns.GpsModeIndicator);
            Assert.AreEqual(Gns.Mode.NoFix, gns.GlonassModeIndicator);
            Assert.AreEqual(0, gns.ModeIndicators.Length);
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
            Assert.IsInstanceOfType(msg, typeof(Gll));
            Gll gll = (Gll)msg;
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
            Assert.IsInstanceOfType(msg, typeof(Bod));
            Bod bod = (Bod)msg;
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
            Assert.IsInstanceOfType(msg, typeof(Bod));
            Bod bod = (Bod)msg;
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
            Assert.IsInstanceOfType(msg, typeof(Bod));
            Bod bod = (Bod)msg;
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
            Assert.IsInstanceOfType(msg, typeof(NmeaParser.Nmea.Garmin.Pgrmz));
            var rmz = (NmeaParser.Nmea.Garmin.Pgrmz)msg;
            Assert.AreEqual(double.NaN, rmz.Altitude, "Altitude");
            Assert.AreEqual(NmeaParser.Nmea.Garmin.Pgrmz.AltitudeUnit.Unknown, rmz.Unit, "Unit");
            Assert.AreEqual(NmeaParser.Nmea.Garmin.Pgrmz.PositionFixType.Unknown, rmz.FixType, "FixDimension");
        }

        [TestMethod]
        public void TestPgrmz()
        {
            string input = "$PGRMZ,93,f,3*21";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(NmeaParser.Nmea.Garmin.Pgrmz));
            var rmz = (NmeaParser.Nmea.Garmin.Pgrmz)msg;
            Assert.AreEqual(93d, rmz.Altitude, "Altitude");
            Assert.AreEqual(NmeaParser.Nmea.Garmin.Pgrmz.AltitudeUnit.Feet, rmz.Unit, "Unit");
            Assert.AreEqual(NmeaParser.Nmea.Garmin.Pgrmz.PositionFixType.Fix3D, rmz.FixType, "FixDimension");
        }

        [TestMethod]
        public void TestGprte()
        {
            string input = "$GPRTE,2,1,c,0,W3IWI,DRIVWY,32CEDR,32-29,32BKLD,32-I95,32-US1,BW-32,BW-198*69";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Rte));
            Rte rte = (Rte)msg;
            Assert.IsFalse(((IMultiSentenceMessage)rte).IsComplete);
            Assert.AreEqual(Rte.WaypointListType.CompleteWaypointsList, rte.ListType);
            Assert.AreEqual("0", rte.RouteId);
            Assert.AreEqual("0", rte.RouteId);
            Assert.AreEqual(9, rte.Waypoints.Count);
            Assert.AreEqual("W3IWI", rte.Waypoints[0]);
            Assert.AreEqual("32BKLD", rte.Waypoints[4]);
            Assert.AreEqual("BW-198", rte.Waypoints[8]);
        }

        [TestMethod]
        public void TestGpgst()
        {
            string input = "$GPGST,172814.0,0.006,0.023,0.020,273.6,0.023,0.020,0.031*6A";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Gst));
            Gst gst = (Gst)msg;
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
            Assert.IsInstanceOfType(msg, typeof(Gst));
            Gst gst = (Gst)msg;
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
            Assert.IsInstanceOfType(msg, typeof(Vtg));
            Vtg vtg = (Vtg)msg;
            Assert.AreEqual(103.85, vtg.CourseTrue);
            Assert.AreEqual(92.79, vtg.CourseMagnetic);
            Assert.AreEqual(0.14, vtg.SpeedKnots);
            Assert.AreEqual(0.25, vtg.SpeedKph);
        }

        [TestMethod]
        public void TestGpvtg_Empty()
        {
            string input = "$GPVTG,,T,,M,0.00,N,0.00,K*4E";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Vtg));
            Vtg vtg = (Vtg)msg;
            Assert.IsTrue(double.IsNaN(vtg.CourseTrue));
            Assert.IsTrue(double.IsNaN(vtg.CourseMagnetic));
            Assert.AreEqual(0.0, vtg.SpeedKnots);
            Assert.AreEqual(0.0, vtg.SpeedKph);
        }

        [TestMethod]
        public void TestGnzda()
        {
            var input = "$GNZDA,075451.00,02,10,2018,00,00*72";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Zda));
            var zda = (Zda)msg;
            Assert.AreEqual(new DateTimeOffset(2018, 10, 02, 07, 54, 51, 00, TimeSpan.Zero), zda.FixDateTime);
        }

        [TestMethod]
        public void TestGpzda()
        {
            var input = "$GPZDA,143042.00,25,08,2005,,*6E";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Zda));
            var zda = (Zda)msg;
            Assert.AreEqual(new DateTimeOffset(2005, 08, 25, 14, 30, 42, 00, TimeSpan.Zero), zda.FixDateTime);
        }

        [TestMethod]
        public void TestGlzda()
        {
            var input = "$GLZDA,225627.00,21,09,2015,00,00*70";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(Zda));
            var zda = (Zda)msg;
            Assert.AreEqual(new DateTimeOffset(2015, 09, 21, 22, 56, 27, 00, TimeSpan.Zero), zda.FixDateTime);
        }

        [TestMethod]
        public void TestCustomMessageRegistration()
        {
            int count = NmeaMessage.RegisterAssembly(typeof(CustomMessage).Assembly, true);
            Assert.AreEqual(1, count);
            var input = "$PTEST,TEST*7C";
            var msg = NmeaMessage.Parse(input);
            Assert.IsInstanceOfType(msg, typeof(CustomMessage));
            var cmsg = (CustomMessage)msg;
            Assert.AreEqual("TEST", cmsg.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestCustomMessageDuplicateRegistrationFailure()
        {
            int count = NmeaMessage.RegisterAssembly(typeof(CustomMessage).Assembly, true);
            Assert.AreEqual(1, count);
            count = NmeaMessage.RegisterAssembly(typeof(CustomMessage).Assembly, false); // This will throw
        }

        [Nmea.NmeaMessageType("PTEST")]
        private class CustomMessage : NmeaMessage
        {
            public CustomMessage(string type, string[] parameters) : base(type, parameters)
            {
                Value = parameters[0];
            }
            public string Value { get; }
        }
    }
}
