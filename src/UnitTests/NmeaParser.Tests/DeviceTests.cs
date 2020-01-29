using Microsoft.VisualStudio.TestTools.UnitTesting;
using NmeaParser.Messages;
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
        [Timeout(2000)]
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
                    Assert.IsInstanceOfType(e.Message, typeof(Gsv));
                    var msg = (NmeaParser.Messages.Gsv)e.Message;
                    Assert.IsTrue(((IMultiSentenceMessage)e.Message).IsComplete);
                    Assert.AreEqual(9, msg.SatellitesInView);
                    Assert.AreEqual(9, msg.SVs.Count);
                    
                    if (count > 1)
                        Assert.Fail();
                    tcs.SetResult(true);
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
        [Timeout(2000)]
        public async Task TestMixedGsvGroupMessage()
        {
            // A group message can have multiple diffent GSV types. 
            string message = @"$GPGSV,4,1,14,26,33,050,43,9,40,314,47,3,55,187,49,23,68,354,48*76
$GPGSV,4,2,14,16,56,082,50,7,28,256,40,6,8,295,34*7E
$GLGSV,4,3,14,73,52,022,47,74,62,248,47,72,44,331,42,71,78,111,49*6A
$GAGSV,4,4,14,19,82,349,40,1,44,220,40,4,24,314,38*5F";
            NmeaDevice dev = new BufferedStringDevice(message);
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            dev.MessageReceived += (s, e) =>
            {
                try
                {
                    Assert.IsInstanceOfType(e.Message, typeof(Gsv));
                    Assert.AreEqual(Talker.Multiple, e.Message.TalkerId);
                    var msg = (Gsv)e.Message;
                    Assert.AreEqual(Talker.GlobalPositioningSystem, msg.SVs[0].TalkerId);
                    Assert.AreEqual(Talker.GlobalPositioningSystem, msg.SVs[4].TalkerId);
                    Assert.AreEqual(Talker.GlonassReceiver, msg.SVs[8].TalkerId);
                    Assert.AreEqual(Talker.GalileoPositioningSystem, msg.SVs[12].TalkerId);
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

        [TestMethod]
        [TestCategory("Device")]
        [Timeout(2000)]
        public async Task TestInvalidGpgsvGroupMessage()
        {
            var message = "$GPGSV,3,2,9,00,30,055,48,00,19,281,00,27,19,275,00,12,16,319,00*4D\n$GPGSV,3,2,9,00,30,055,48,00,19,281,00,27,19,275,00,12,16,319,00*4F\n$GPGSV,3,3,9,32,10,037,00,,,,,,,,,,,,*74";
            NmeaDevice dev = new BufferedStringDevice(message);
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            bool messageRecieved = false;
            dev.MessageReceived += (s, e) =>
            {
                messageRecieved = true;
            };
            await dev.OpenAsync();
            await Task.Delay(100);
            var _ = dev.CloseAsync();
            if (messageRecieved)
                Assert.Fail("Event shouldn't be raised for incomplete messages");
        }
    }
}
