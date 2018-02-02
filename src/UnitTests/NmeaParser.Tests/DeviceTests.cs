using Microsoft.VisualStudio.TestTools.UnitTesting;
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
					Assert.IsInstanceOfType(e.Message, typeof(NmeaParser.Nmea.Gps.Gpgsv));
					var msg = e.Message as NmeaParser.Nmea.Gps.Gpgsv;
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
					Assert.IsInstanceOfType(e.Message, typeof(NmeaParser.Nmea.Gps.Gpgsv));
					var msg = e.Message as NmeaParser.Nmea.Gps.Gpgsv;
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
