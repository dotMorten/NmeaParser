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

#if __IOS__
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExternalAccessory;

namespace NmeaParser
{
    /// <summary>
    /// Creates an object to read from an iOS accessory like a Bluetooth device.
    /// </summary>
    /// <remarks>
    /// <para>It's worth nothing that iOS is very limited to what devices it can connect to, and generally
    /// needs to be MFI certified devices.</para>
    /// <para>
    /// To connect to a device in an iOS app, the device must be supported by iOS and enabled in <c>Info.plist</c>.
    /// As an example you can declare a Bad Elf GPS receiver like this in the plist:
    /// </para>
    /// <code lang="xml">
    ///   &lt;key>UISupportedExternalAccessoryProtocols&lt;/key>
    ///   &lt;array>
    ///     &lt;string>com.bad-elf.gps&lt;/string>
    ///   &lt;/array>
    /// </code>
    /// <para>
    /// If using a bad-elf GPS, make sure you send the configuration to enable sending NMEA data.
    /// Example:
    /// <code lang="cs">
    /// var device = new EAAccessoryDevice(accessory, "com.bad-elf.gps");
    /// await device.OpenAsync();
    /// // BadElf start packet.
    /// // See https://github.com/BadElf/gps-sdk/blob/master/README.md#extended-nmea-sentences-with-gsa-and-gsv-satellite-data-and-dop-values
    /// byte[] startPacket = new byte[] { 0x24, 0xbe, 0x00, 0x11, 0x16, 0x01, 0x02, 0xf4, 0x31, 0x0a, 0x32, 0x04, 0x33, 0x02, 0x5a, 0x0d, 0x0a };
    /// await device.WriteAsync(startPacket, 0, startPacket.Length);
    /// </code>
    /// </para>
    /// </remarks>
    public class EAAccessoryDevice : NmeaDevice
    {
        private readonly EAAccessory m_accessory;
        private readonly string m_protocol;

        /// <summary>
        /// Gets a list of supported known EAAccessories that support NMEA
        /// </summary>
        /// <remarks>
        /// <para>Returns a list of devices. Only devices enabled in the <c>Info.plist</c> will be enabled. See below for an example:</para>
        /// <code lang="xml">
        ///   &lt;key>UISupportedExternalAccessoryProtocols&lt;/key>
        ///   &lt;array>
        ///     &lt;string>com.bad-elf.gps&lt;/string>
        ///   &lt;/array>
        /// </code>
        /// <para>Supported protocols:</para>
        /// <list type="bullet">
        /// <item>Bad Elf: <c>com.bad-elf.gps</c>.</item>
        /// <item>Generic bluetooth serial: <c> 00001101-0000-1000-8000-00805F9B34FB</c></item>
        /// </list>
        /// <para>If you know of other MFI certificed NMEA devices, please make a request to have it added here: https://github.com/dotMorten/NmeaParser/issues/new
        /// or you can use <c>EAAccessoryManager.SharedAccessoryManager.ConnectedAccessories</c> to iterate and find the devices yourself.</para>
        /// </remarks>
        /// <returns>A list of supported devices</returns>
        public static IEnumerable<EAAccessoryDevice> GetDevices()
        {
            foreach (var accessory in EAAccessoryManager.SharedAccessoryManager.ConnectedAccessories)
            {
                if (accessory.ProtocolStrings.Contains("com.bad-elf.gps"))
                {
                    yield return new EAAccessoryDevice(accessory, "com.bad-elf.gps");
                }
                else if (accessory.ProtocolStrings.Contains("com.dualav.xgps150"))
                {
                    yield return new EAAccessoryDevice(accessory, "com.dualav.xgps150");
                }                
                else if (accessory.ProtocolStrings.Contains("00001101-0000-1000-8000-00805F9B34FB"))
                {
                    yield return new EAAccessoryDevice(accessory, "00001101-0000-1000-8000-00805F9B34FB");
                }
                // TODO: Add more known devices here.
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EAAccessoryDevice"/> class.
        /// </summary>
        /// <param name="accessory"></param>
        /// <param name="protocol"></param>
        public EAAccessoryDevice(EAAccessory accessory, string protocol)
        {
            m_accessory = accessory;
            m_protocol = protocol;
        }

        /// <inheritdoc />
        protected override Task CloseStreamAsync(Stream stream)
        {
            stream.Dispose();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override Task<Stream> OpenStreamAsync() 
            => Task.FromResult<Stream>(new EAAccessoryStream(m_accessory, m_protocol));

        private class EAAccessoryStream : Stream
        {
            private readonly EASession m_session;

            public EAAccessoryStream(EAAccessory accessory, string protocol)
            {
                m_session = new EASession(accessory, protocol);
                m_session.InputStream?.Open();
                m_session.OutputStream?.Open();
            }

            ~EAAccessoryStream()
            {
                Dispose(false);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    m_session.InputStream?.Close();
                    m_session.InputStream?.Dispose();
                    m_session.OutputStream?.Close();
                    m_session.OutputStream?.Dispose();
                    m_session.Dispose();
                }
                base.Dispose(disposing);
            }

            public override bool CanRead => m_session.InputStream != null;

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (m_session.InputStream == null)
                    throw new NotSupportedException();
                if (!m_session.InputStream.HasBytesAvailable())
                    return 0;
                return (int)m_session.InputStream.Read(buffer, offset, (nuint)count);
            }

            public override bool CanWrite => m_session.OutputStream != null;

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (m_session.OutputStream == null)
                    throw new NotSupportedException();
                m_session.OutputStream.Write(buffer, offset, (nuint)count);
            }
            
            public override bool CanSeek => false;
            
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override long Length => -1;

            public override void SetLength(long value) => throw new NotSupportedException();

            public override long Position 
            {
                get => throw new NotSupportedException(); 
                set => throw new NotSupportedException(); 
            }

            public override void Flush()
            {
            }
        }
    }
}
#endif