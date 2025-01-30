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

using System;

namespace NmeaParser.Messages
{
    /// <summary>
    ///     Measurement data from transducers that measure physical quantities such as 
    ///     temperature, force, pressure, frequency, angular or linear displacement, etc. 
    ///     Four fields 'Type-Data-Units-ID'
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         1.: Transducer type
    ///         2.: Measurement data
    ///         3.: units of measure
    ///         4.: transducer ID
    ///     </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xdr")]
    [NmeaMessageType("--XDR")]
    public class Xdr : NmeaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Xdr"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Xdr(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 4)
                throw new ArgumentException("Invalid Xdr", "message");
            Type = message[0];
            Data = StringToDouble(message[1]);
            Unit = message[2];
            ID = message[3];
        }

        /// <summary>
        ///     Transducer Type
        /// </summary>
        public string Type { get; }

        /// <summary>
        ///     Measurement data
        /// </summary>
        public double Data { get; }

        /// <summary>
        ///     Unit of measure
        /// </summary>
        public string Unit { get; }

        /// <summary>
        ///     ID
        /// </summary>
        public string ID { get; }
    }
}