//  *******************************************************************************
//  *  Licensed under the Apache License, Version 2.0 (the "License");
//  *  you may not use this file except in compliance with the License.
//  *  You may obtain a copy of the License at
//  *
//  *  http://www.apache.org/licenses/LICENSE-2.0
//  *
//  *   Unless required by applicable law or agreed to in writing, software
//  *   distributed under the License is distributed on an "AS IS" BASIS,
//  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *   See the License for the specific language governing permissions and
//  *   limitations under the License.
//  ******************************************************************************

#if NETFX_CORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Devices.Bluetooth.Rfcomm;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Devices.Enumeration;
using Windows.Networking.Proximity;

namespace NmeaParser
{
    /// <summary>
    /// A Bluetooth NMEA device
    /// </summary>
    /// <remarks>
    /// To use the NMEA Parser against a bluetooth device in a Universal App,
    /// ensure the bluetooth capability is enabled by opening <c>package.appxmanifest</c> in a text editor,
    /// and add the following to the <c>&lt;Capabilities></c> section:
    /// <code lang="xml">
    /// &lt;DeviceCapability Name="bluetooth.rfcomm">
    ///   &lt;Device Id="any">
    ///     &lt;Function Type="name:serialPort" />
    ///   &lt;/Device>
    /// &lt;/DeviceCapability>
    /// </code>
    /// <para>
    /// See more here on bluetooth device capabilities in UWP Apps: https://docs.microsoft.com/en-us/uwp/schemas/appxpackage/how-to-specify-device-capabilities-for-bluetooth
    /// </para>
    /// <para>Make sure your Bluetooth device is paired with your Windows Device.</para>
    /// <code lang="cs">
    /// //Get list of devices
    ///     string serialDeviceType = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
    ///     var devices = await DeviceInformation.FindAllAsync(serialDeviceType);
    ///     //Select device by name (in this case TruePulse 360B Laser Range Finder)
    ///     var TruePulse360B = devices.Where(t => t.Name.StartsWith("TP360B-")).FirstOrDefault();
    ///     //Get service
    ///     RfcommDeviceService rfcommService = await RfcommDeviceService.FromIdAsync(TruePulse360B.Id);
    /// if (rfcommService != null)
    /// {
    /// 	var rangeFinder = new NmeaParser.BluetoothDevice(rfcommService);
    ///     rangeFinder.MessageReceived += device_NmeaMessageReceived;
    /// 	await rangeFinder.OpenAsync();
    /// }
    /// ...
    /// private void device_NmeaMessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs args)
    /// {
    ///     // called when a message is received
    /// }
    /// </code>
    /// </remarks>
    public class BluetoothDevice : NmeaDevice
    {
        private Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService? m_deviceService;
        private Windows.Networking.Proximity.PeerInformation? m_devicePeer;
        private StreamSocket? m_socket;
        private bool m_disposeService;
        private SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Gets a list of bluetooth devices that supports serial communication
        /// </summary>
        /// <returns>A set of bluetooth devices available that supports serial connections</returns>
        public static async Task<IEnumerable<RfcommDeviceService>> GetBluetoothSerialDevicesAsync()
        {
            string serialDeviceType = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
            var devices = await DeviceInformation.FindAllAsync(serialDeviceType);
            List<RfcommDeviceService> services = new List<RfcommDeviceService>();
            foreach(var d in devices)
                services.Add(await RfcommDeviceService.FromIdAsync(d.Id));
            return services;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothDevice"/> class.
        /// </summary>
        /// <param name="service">The RF Comm Device service.</param>
        /// <param name="disposeService">Whether this devicee should also dispose the RfcommDeviceService provided when this device disposes.</param>
        public BluetoothDevice(RfcommDeviceService service, bool disposeService = false)
        {
            m_deviceService = service ?? throw new ArgumentNullException(nameof(service));
            m_disposeService = disposeService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothDevice"/> class.
        /// </summary>
        /// <param name="peer">The peer information device.</param>
        public BluetoothDevice(Windows.Networking.Proximity.PeerInformation peer)
        {
            m_devicePeer = peer ?? throw new ArgumentNullException(nameof(peer));
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (m_disposeService && m_deviceService != null)
                m_deviceService.Dispose();
            m_deviceService = null;
            m_devicePeer = null;
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override async Task<System.IO.Stream> OpenStreamAsync()
        {
            var socket = new Windows.Networking.Sockets.StreamSocket();
            socket.Control.KeepAlive = true;
            if (m_devicePeer != null)
            {
                await socket.ConnectAsync(m_devicePeer.HostName, "1");
            }
            else if (m_deviceService != null)
            {
                await socket.ConnectAsync(m_deviceService.ConnectionHostName, m_deviceService.ConnectionServiceName);
            }
            else
                throw new InvalidOperationException();
            m_socket = socket;
            
            return new DummyStream(); //We're going to use WinRT buffers instead and will handle read/write, so no reason to return a real stream. This is mainly done to avoid locking issues reading and writing at the same time
        }

        private class DummyStream : Stream
        {
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override void Flush() => throw new NotSupportedException();
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override Task CloseStreamAsync(System.IO.Stream stream)
        {
            if(m_socket == null)
                throw new InvalidOperationException("No connection to close");
            m_socket.Dispose();
            m_socket = null;
            return Task.FromResult(true);
        }


        /// <inheritdoc />
        protected override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // Reading and writing to the Bluetooth serial connection at the same time seems very unstable in UWP,
            // so we use a semaphore to ensure we don't read and write at the same time
            await m_semaphoreSlim.WaitAsync().ConfigureAwait(false);
            if (m_socket == null)
                throw new InvalidOperationException("Socket not initialized");
            try
            {
                var r = await m_socket.InputStream.ReadAsync(buffer.AsBuffer(), (uint)count, Windows.Storage.Streams.InputStreamOptions.None);
                return (int)r.Length;
            }
            finally
            {
                m_semaphoreSlim.Release();
            }
        }

        /// <inheritdoc />
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override async Task WriteAsync(byte[] buffer, int offset, int length)
        {
            if (m_socket == null)
                throw new InvalidOperationException("Device not open");
            // Reading and writing to the Bluetooth serial connection at the same time seems very unstable in UWP,
            // so we use a semaphore to ensure we don't read and write at the same time
            await m_semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                await m_socket.OutputStream.WriteAsync(buffer.AsBuffer(offset, length)).AsTask().ConfigureAwait(false);
            }
            finally
            {
                m_semaphoreSlim.Release();
            }
        }
    }
}
#endif