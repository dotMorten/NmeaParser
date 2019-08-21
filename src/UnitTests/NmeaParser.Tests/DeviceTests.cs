using Microsoft.VisualStudio.TestTools.UnitTesting;
using NmeaParser.Nmea;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser.Tests
{
	[TestClass]
	public class DeviceTests
	{
		[TestMethod]
		[TestCategory("Device")]
		public async Task TestGpgsvGroupMessage()
		{
			var message = "$GPGSV,3,1,9,00,30,055,48,00,19,281,00,27,19,275,00,12,16,319,00*4C\n$GPGSV,3,2,9,00,30,055,48,00,19,281,00,27,19,275,00,12,16,319,00*4F\n$GPGSV,3,3,9,32,10,037,00,,,,,,,,,,,,*74";
			NmeaDevice dev = new BufferedStringDevice(message);
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			int count = 0;
			dev.MessageReceived += (s, e) =>
			{
				count++;
				try
				{
					Assert.IsTrue(e.IsMultipart, "IsMultiPart");
					Assert.IsInstanceOfType(e.Message, typeof(NmeaParser.Nmea.Gsv));
					var msg = (NmeaParser.Nmea.Gsv)e.Message;
					if (msg.TotalMessages == msg.MessageNumber)
					{
						Assert.IsNotNull(e.MessageParts);
						Assert.AreEqual(e.MessageParts.Count, 3, "MessageParts.Length");
						tcs.SetResult(true);
					}
					else
						Assert.IsNull(e.MessageParts);
					if (count > 3)
						Assert.Fail();
				}
				catch(System.Exception ex)
				{
					tcs.SetException(ex);
				}
			};
			await dev.OpenAsync();
			await tcs.Task;
			var _ = dev.CloseAsync();
		}

        [TestMethod]
        [TestCategory("Device")]
        public async Task TestMixedGsvGroupMessage()
        {
            // A group message can have multiple diffent GSV types. 
            string message = @"$GPGSV,4,1,14,26,33,050,43,9,40,314,47,3,55,187,49,23,68,354,48*76
$GPGSV,4,2,14,16,56,082,50,7,28,256,40,6,8,295,34*7E
$GLGSV,4,3,14,73,52,022,47,74,62,248,47,72,44,331,42,71,78,111,49*6A
$GAGSV,4,4,14,19,82,349,40,1,44,220,40,4,24,314,38*5F";
            NmeaDevice dev = new BufferedStringDevice(message);
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            int count = 0;
            dev.MessageReceived += (s, e) =>
            {
                count++;
                try
                {
                    Assert.IsTrue(e.IsMultipart, "IsMultiPart");
                    Assert.IsInstanceOfType(e.Message, typeof(NmeaParser.Nmea.Gsv));
                    var msg = (NmeaParser.Nmea.Gsv)e.Message;
                    if (msg.TotalMessages == msg.MessageNumber)
                    {
                        Assert.IsNotNull(e.MessageParts);
                        Assert.AreEqual(e.MessageParts.Count, 4, "MessageParts.Length");
                        Assert.IsInstanceOfType(e.MessageParts[0], typeof(NmeaParser.Nmea.Gsv));
                        Assert.IsInstanceOfType(e.MessageParts[1], typeof(NmeaParser.Nmea.Gsv));
                        Assert.IsInstanceOfType(e.MessageParts[2], typeof(NmeaParser.Nmea.Gsv));
                        Assert.IsInstanceOfType(e.MessageParts[3], typeof(NmeaParser.Nmea.Gsv));
                        Assert.AreEqual(Talker.GlobalPositioningSystem, e.MessageParts[0].TalkerId);
                        Assert.AreEqual(Talker.GlobalPositioningSystem, e.MessageParts[1].TalkerId);
                        Assert.AreEqual(Talker.GlonassReceiver, e.MessageParts[2].TalkerId);
                        Assert.AreEqual(Talker.GalileoPositioningSystem, e.MessageParts[3].TalkerId);

                        tcs.SetResult(true);
                    }
                    else
                        Assert.IsNull(e.MessageParts);
                    if (count > 3)
                        Assert.Fail();
                }
                catch (System.Exception ex)
                {
                    tcs.SetException(ex);
                }
            };
            await dev.OpenAsync();
            await tcs.Task;
            var _ = dev.CloseAsync();
        }

        [TestMethod]
		[TestCategory("Device")]
		public async Task TestInvalidGpgsvGroupMessage()
		{
			var message = "$GPGSV,3,2,9,00,30,055,48,00,19,281,00,27,19,275,00,12,16,319,00*4D\n$GPGSV,3,2,9,00,30,055,48,00,19,281,00,27,19,275,00,12,16,319,00*4F\n$GPGSV,3,3,9,32,10,037,00,,,,,,,,,,,,*74";
			NmeaDevice dev = new BufferedStringDevice(message);
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			int count = 0;
			dev.MessageReceived += (s, e) =>
			{
				count++;
				try
				{
					Assert.IsTrue(e.IsMultipart, "IsMultiPart");
					Assert.IsInstanceOfType(e.Message, typeof(NmeaParser.Nmea.Gsv));
					var msg = e.Message as NmeaParser.Nmea.Gsv;
					Assert.IsNull(e.MessageParts);
					if (count > 6)
						tcs.SetResult(true);
				}
				catch (System.Exception ex)
				{
					tcs.SetException(ex);
				}
			};
			await dev.OpenAsync();
			await tcs.Task;
			var _ = dev.CloseAsync();
		}
	}
}
