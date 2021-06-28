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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using NmeaParser.Nmea.Class;

namespace NmeaParser.Messages
{
    /// <summary>
    ///     Rate and direction of turn
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         1.: Rate of turn, degrees/minute, "-" = bow turns to port
    ///         2.: Status A = Data valid, V = Data invalid
    ///     </para>
    ///     <para>
    ///         Rate of turn and direction of turn. 
    ///     </para>
    ///     <para><see cref="Rot" /> and <see cref="Rot" /> are the recommended minimum data to be provided by a GNSS receiver.</para>
    /// </remarks>
    ///
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "GpRot")][NmeaMessageType("--ROT")]
    public class Rot : NmeaMessage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Rot" /> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Rot(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 2)
                throw new ArgumentException("Invalid Rot", "message");

            RateOfTurn = double.TryParse(message[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var tmp)
                ? tmp
                : double.NaN;
            Validation = message[1] == "A" ? Validation.Ok : Validation.Warning;
        }

        /// <summary>
        ///     Rate of turn, degrees/minutes, “–” indicates bow turns to port
        /// </summary>
        public double RateOfTurn { get; }

        /// <summary>
        ///     Status A = Data valid, V = Data invalid
        /// </summary>
        public Validation Validation { get; }
    }
}

