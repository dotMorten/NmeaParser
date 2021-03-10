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

#if WINDOWS_UWP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using System.Runtime.InteropServices.WindowsRuntime;

namespace NmeaParser
{
    /// <summary>
    /// A Serial Port NMEA device
    /// </summary>
    /// <remarks>
    /// <para>
    /// To use the NMEA Parser against a serial device in a Windows 10 Universal app, ensure the serial device capability is enabled by opening package.appxmanifest in a text editor, and add the following to the <c>&lt;Capabilities></c> section:
    /// </para>
    /// <code lang="xml">
    ///    &lt;DeviceCapability Name="serialcommunication"> 
    ///      &lt;Device Id="any">
    ///        &lt;Function Type="name:serialPort" /> 
    ///      &lt;/Device> 
    ///    &lt;/DeviceCapability>
    /// </code>
    /// <code lang="cs">
    /// var selector = SerialDevice.GetDeviceSelector("COM3"); //Get the serial port on port '3'
    /// var devices = await DeviceInformation.FindAllAsync(selector);
    /// if(devices.Any()) //if the device is found
    /// {
    /// 	var deviceInfo = devices.First();
    ///     var serialDevice = await SerialDevice.FromIdAsync(deviceInfo.Id);
    ///     //Set up serial device according to device specifications:
    ///     //This might differ from device to device
    ///     serialDevice.BaudRate = 4800;
    /// 	serialDevice.DataBits = 8;
    /// 	serialDevice.Parity = SerialParity.None;
    /// 	var device = new NmeaParser.SerialPortDevice(serialDevice);
    ///     device.MessageReceived += device_NmeaMessageReceived;
    /// }
    /// ...
    /// private void device_NmeaMessageReceived(NmeaParser.NmeaDevice sender, NmeaMessageReceivedEventArgs args)
    /// {
    ///     // called when a message is received
    /// }
    /// </code>
    /// </remarks>
    public class SerialPortDevice : NmeaDevice
    {
        private SerialDevice m_port;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialPortDevice" /> class.
        /// </summary>
        /// <param name="device">The serial port.</param>
        /// <exception cref="ArgumentNullException">port</exception>
        public SerialPortDevice(SerialDevice device)
        {
            if (device == null)
                throw new ArgumentNullException("device");
            m_port = device;
        }

        /// <summary>
        /// Gets the active serial port.
        /// </summary>
        public SerialDevice SerialDevice
        {
            get
            {
                return m_port;
            }
        }

        /// <inheritdoc />
        protected override Task<System.IO.Stream> OpenStreamAsync()
        {
            return Task.FromResult<System.IO.Stream>(m_port.InputStream.AsStreamForRead(0));
        }

        /// <inheritdoc />
        protected override Task CloseStreamAsync(System.IO.Stream stream)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes data to the serial port (useful for RTCM/dGPS scenarios)
        /// </summary>
        /// <param name="buffer">The byte array that contains the data to write to the port.</param>
        /// <param name="offset">The zero-based byte offset in the buffer parameter at which to begin copying 
        /// bytes to the port.</param>
        /// <param name="count">The number of bytes to write.</param>
        [Obsolete("Use WriteAsync")]
        public void Write(byte[] buffer, int offset, int count)
        {
            m_port.OutputStream.AsStreamForWrite().Write(buffer, offset, count);
        }

        /// <inheritdoc />
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override Task WriteAsync(byte[] buffer, int offset, int length)
        {
            if (m_port == null)
                throw new InvalidOperationException("Device not open");

            return m_port.OutputStream.WriteAsync(buffer.AsBuffer(offset, length)).AsTask();
        }
    }
}
#endif