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
    ///     AML Sound Velocity (m/s), Temperature (C)
    ///     AML Oceanographic Svm sensor
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         1.: Sound Velocity m/s
    ///         2.: Unit of temperature, TC = Celcius
    ///         3.: Temperature
    ///         4.: Label, SN: Serial number
    ///         5.: Device serial number
    ///     </para>
    /// </remarks>
    [NmeaMessageType("SVM")]
    public class Svm : NmeaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Svm"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Svm(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 5)
                throw new ArgumentException("Invalid Svm", "message");
            SoundVelocity = StringToDouble(message[0]);
            TemperatureUnit = message[1];
            Temperature = StringToDouble(message[2]);
            IsSerialNumber = message[3] == "SN";
            SerialNumber = StringToDouble(message[4]);
        }

        /// <summary>
        ///     Sound Velocity m/s
        /// </summary>
        public double SoundVelocity { get; }

        /// <summary>
        ///     Temperature Unit
        /// </summary>
        public string TemperatureUnit { get; }

        /// <summary>
        ///     Temperature
        /// </summary>
        public double Temperature { get; }

        /// <summary>
        ///     String indicating Serial number
        /// </summary>
        public bool IsSerialNumber { get; }

        /// <summary>
        ///     Serial Number
        /// </summary>
        public double SerialNumber { get; }
    }
}