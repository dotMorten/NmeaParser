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

#if __ANDROID__
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Android.Bluetooth;

namespace NmeaParser
{
    /// <summary>
    /// A Bluetooth NMEA device
    /// </summary>
    /// <remarks>
    /// To use this device, ensure you have the necessary permissions in the <c>AndroidManifest.xml</c> file:
    /// <code lang="xml">
    /// ```xml
    /// 	&lt;uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
    /// 	&lt;uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
    /// 	&lt;uses-permission android:name="android.permission.BLUETOOTH" />
    /// ```
    /// </code>
    /// <para>
    /// Next see MainActivity.cs in the Android sample as a reference:
    /// https://github.com/dotMorten/NmeaParser/blob/main/src/SampleApp.Droid/MainActivity.cs
    /// </para>
    /// </remarks>
    public class BluetoothDevice : NmeaDevice
    {
        private static Java.Util.UUID SERIAL_UUID = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB")!;
        private readonly Android.Bluetooth.BluetoothDevice m_device;
        private BluetoothSocket? m_socket;
        private readonly Android.Content.Context? m_context;

        /// <summary>
        /// Gets a list of bluetooth devices that supports serial communication
        /// </summary>
        /// <returns>A set of bluetooth devices available that supports serial connections</returns>
        [Obsolete("Use GetBluetoothSerialDevices(Context)")]
        public static IEnumerable<Android.Bluetooth.BluetoothDevice> GetBluetoothSerialDevices()
        {
            var adapter = Android.Bluetooth.BluetoothAdapter.DefaultAdapter;
            if (adapter != null && adapter.IsEnabled && adapter.BondedDevices is not null)
            {
                foreach (var b in adapter.BondedDevices.Where(d => d.GetUuids()?.Any(t => SERIAL_UUID.CompareTo(t.Uuid) == 0) == true))
                    yield return b;
            }
        }

        /// <summary>
        /// Gets a list of bluetooth devices that supports serial communication
        /// </summary>
        /// <param name="context">The Android context</param>
        /// <returns>A set of bluetooth devices available that supports serial connections</returns>
        public static IEnumerable<Android.Bluetooth.BluetoothDevice> GetBluetoothSerialDevices(Android.Content.Context context)
        {
            var bluetoothManager = context.GetSystemService(Android.Content.Context.BluetoothService) as BluetoothManager;
            var adapter = bluetoothManager?.Adapter;
            if (adapter != null && adapter.IsEnabled && adapter.BondedDevices is not null)
            {
                foreach (var b in adapter.BondedDevices.Where(d => d.GetUuids()?.Any(t => SERIAL_UUID.CompareTo(t.Uuid) == 0) == true))
                    yield return b;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothDevice"/> class.
        /// </summary>
        /// <param name="device">The Android Bluetooth Device.</param>
        [Obsolete("Use BluetoothDevice(BluetoothDevice, Context)")]
        public BluetoothDevice(Android.Bluetooth.BluetoothDevice device)
        {
            m_device = device ?? throw new ArgumentNullException(nameof(device));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothDevice"/> class.
        /// </summary>
        /// <param name="device">The Android Bluetooth Device.</param>
        /// <param name="context">The Android context</param>
        public BluetoothDevice(Android.Bluetooth.BluetoothDevice device, Android.Content.Context context)
        {
            m_device = device ?? throw new ArgumentNullException(nameof(device));
            m_context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        protected override async Task<System.IO.Stream> OpenStreamAsync()
        {
            BluetoothAdapter? adapter;
            if (m_context is not null)
            {
                var bluetoothManager = m_context.GetSystemService(Android.Content.Context.BluetoothService) as BluetoothManager;
                adapter = bluetoothManager?.Adapter;
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete. Only used if deprecated constructor is used
                adapter = Android.Bluetooth.BluetoothAdapter.DefaultAdapter;
#pragma warning restore CS0618 // Type or member is obsolete
            }
            if (adapter?.IsEnabled != true)
                throw new InvalidOperationException("Bluetooth Adapter not enabled");
            var d = adapter.GetRemoteDevice(m_device.Address);
            if( d == null)
                throw new InvalidOperationException($"Failed to get Remove device '{m_device.Address}'");
            var socket = d.CreateRfcommSocketToServiceRecord(SERIAL_UUID);
            if (socket == null)
                throw new InvalidOperationException($"Failed to create socket");
            try
            {
                await socket.ConnectAsync();
            }
            catch (Java.IO.IOException)
            {
                // This sometimes fails. Use fallback approach to open socket
                // Based on https://stackoverflow.com/a/41627149
                socket?.Dispose();
                var m = d.Class?.GetMethod("createRfcommSocket", new Java.Lang.Class[] { Java.Lang.Integer.Type! })!;
                if (m is null)
                    throw;
                socket = (Android.Bluetooth.BluetoothSocket)m.Invoke(d, new Java.Lang.Object[] { 1 })!;
                socket.Connect();
            }
            m_socket = socket;
            if (socket.InputStream == null)
                throw new InvalidOperationException($"Failed to create socket input stream");
            return socket.InputStream;
        }

        /// <inheritdoc />
        protected override Task CloseStreamAsync(System.IO.Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            stream.Dispose();
            m_socket?.Dispose();
            m_socket = null;
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override Task WriteAsync(byte[] buffer, int offset, int length)
        {
            if (m_socket == null)
                throw new InvalidOperationException("Device not open");
            if (m_socket.OutputStream == null)
                throw new InvalidOperationException("Device does not support writes");
            return m_socket.OutputStream.WriteAsync(buffer, offset, length);
        }
    }
}
#endif
