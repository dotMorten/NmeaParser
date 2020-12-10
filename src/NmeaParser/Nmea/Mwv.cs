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
    ///     Relative (Apparent) Wind Speed and Angle
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         1.: Wind Angle, 0 to 360 degrees
    ///         2.: Reference, R = Relative, T = True
    ///         3.: Wind Speed
    ///         4.: Wind Speed Units, K/M/N
    ///         5.: Validation, A = Data Valid
    ///         6.: Checksum
    ///     </para>
    ///     <para>
    ///         Wind angle in relation to the vessel's heading and wind speed measured relative to the moving vessel
    ///     </para>
    /// </remarks>
    ///
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "GpMwv")][NmeaMessageType("--MWV")]
    public class Mwv : NmeaMessage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Mwv" /> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Mwv(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 5)
                throw new ArgumentException("Invalid Mwv", "message");

            WindAngle = double.TryParse(message[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var tmp)
                ? tmp
                : double.NaN;
            Reference = message[1] == "R" ? Reference.Relative : Reference.True;

            WindSpeed = double.TryParse(message[2], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;

            WindSpeedUnit = message[3] switch
            {
                "K" => Unit.Kmh,
                "M" => Unit.Mile,
                "N" => Unit.Knot,
                _ => WindSpeedUnit
            };
            Validation = message[4] == "A" ? Validation.Ok : Validation.Warning;
        }

        /// <summary>
        ///     Wind Angle, 0 to 360 degrees
        /// </summary>
        public double WindAngle { get; }

        /// <summary>
        ///      Reference, R = Relative, T = True
        /// </summary>
        public Reference Reference { get; }

        /// <summary>
        ///      Wind Speed
        /// </summary>
        public double WindSpeed { get; }

        /// <summary>
        ///     Wind Speed Units, K/M/N
        /// </summary>
        public Unit WindSpeedUnit { get; }

        /// <summary>
        ///     Validation, A = Data Valid
        /// </summary>
        public Validation Validation { get; }
    }
}