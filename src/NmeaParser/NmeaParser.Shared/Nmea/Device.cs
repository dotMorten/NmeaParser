using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
#if NETFX_CORE
using BTDevice = Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
#else
using BTDevice = Windows.Networking.Proximity.PeerInformation;
#endif

namespace NmeaParser
{
	public abstract class Device : IDisposable
	{
		private BTDevice m_device;
		private StreamSocket socket;
		System.Threading.CancellationTokenSource tcs;
		public Device(BTDevice device)
		{
			if (device == null)
				throw new ArgumentNullException("device");
#if NETFX_CORE
			if (device.ServiceId.Uuid != RfcommServiceId.SerialPort.Uuid)
				throw new NotSupportedException("Only SerialPort devices supported");
#endif
			m_device = device;
		}
		TaskCompletionSource<bool> closeTask;

		public async Task StartAsync()
		{
			if (tcs != null)
				return;
			if (m_device == null)
				throw new ObjectDisposedException("Device");
			tcs = new System.Threading.CancellationTokenSource();
			socket = new StreamSocket();
			await socket.ConnectAsync(
#if NETFX_CORE
								m_device.ConnectionHostName,
								m_device.ConnectionServiceName);
#else
								m_device.HostName, "1");
			//socket = await Windows.Networking.Proximity.PeerFinder.ConnectAsync(m_device.HostName;
#endif
			if (tcs.IsCancellationRequested) //Stop was called while opening device
			{
				socket.Dispose();
				socket = null;
				throw new TaskCanceledException();
			} var token = tcs.Token;
			var _ = Task.Run(async () =>
			{
				var stream = socket.InputStream.AsStreamForRead();
				byte[] buffer = new byte[1024];
				while (!token.IsCancellationRequested)
				{
					int readCount = 0;
					try
					{
						readCount = await stream.ReadAsync(buffer, 0, 1024, token).ConfigureAwait(false);
					}
					catch { }
					if (token.IsCancellationRequested)
						break;
					if (readCount > 0)
					{
						OnData(buffer.Take(readCount).ToArray());
					}
					await Task.Delay(10, token);
				}
				if(socket != null)
					socket.Dispose();
				if (closeTask != null)
					closeTask.SetResult(true);
			});
		}

		public Task StopAsync()
		{
			if (tcs != null)
			{
				socket.Dispose();
				socket = null;
				closeTask = new TaskCompletionSource<bool>();
				if(tcs != null)
					tcs.Cancel();
				tcs = null;
				return closeTask.Task;
			}
			return Task.FromResult(false);
		}

		protected abstract void OnData(byte[] data);

		public void Dispose()
		{
			if (tcs != null)
			{
				tcs.Cancel();
				tcs = null;
			}
			m_device = null;
			if(socket != null)
				socket.Dispose();
			socket = null;
		}
	}
}
