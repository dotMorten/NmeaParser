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
    ///     Water Current Layer 
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         1.: Validity of the data, A= Valid, V= not valid 
    ///         2.:	Data set number, 0 to 9
    ///         3.: Layer number
    ///         4.: Current depth in meters
    ///         5.: Current direction in degrees
    ///         6.: Direction reference in use, True/Relative T/R
    ///         7.:	Current Speed in Knots
    ///         8.:	Reference layer depth in meters
    ///         9.:	Heading
    ///         10.:Heading reference in use, True/Magnetic T/M
    ///         11.:Speed reference, B : Bottom track, W: Water track, P : Positioning System
    ///     </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "StCur")]
    [NmeaMessageType("--CUR")]
    public class Cur : NmeaMessage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Cur" /> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Cur(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 9)
                throw new ArgumentException("Invalid Cur", "message");

            Validation = message[0] == "A" ? Validation.Ok : Validation.Warning;

            DataSetNo = double.TryParse(message[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var tmp)
                ? tmp
                : double.NaN;

            LayerNo = double.TryParse(message[2], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;

            CurrentDepthMeter = double.TryParse(message[3], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;

            CurrentDirDeg = double.TryParse(message[4], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;

            DirectionRefInUse =
                message[5] == "T" ? Reference.True : Reference.Relative;

            CurrentSpeedKnots = double.TryParse(message[6], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;

            RefLayerDepthMeters = double.TryParse(message[7], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;

            Heading = double.TryParse(message[8], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;

            HeadingRefInUse = message[9] == "T" ? Reference.True : Reference.Magnetic;

            SpeedRefInUse = message[10] switch
            {
                "B" => SpeedReference.BottomTrack,
                "W" => SpeedReference.WatherTrack,
                "P" => SpeedReference.PosSystem,
                _ => SpeedRefInUse
            };
        }

        /// <summary>
        ///     Data Validation
        /// </summary>
        public Validation Validation { get; }

        /// <summary>
        ///     Data set number, 0 to 9
        /// </summary>
        public double DataSetNo { get; }

        /// <summary>
        ///     Layer number
        /// </summary>
        public double LayerNo { get; }

        /// <summary>
        ///     Current depth in meters
        /// </summary>
        public double CurrentDepthMeter { get; }

        /// <summary>
        ///     Current direction in degrees
        /// </summary>
        public double CurrentDirDeg { get; }

        /// <summary>
        ///     Direction reference in use, True/Relative T/R
        /// </summary>
        public Reference DirectionRefInUse { get; }

        /// <summary>
        ///     Range to destination in nautical miles
        /// </summary>
        public double CurrentSpeedKnots { get; }

        /// <summary>
        ///     Reference layer depth in meters
        /// </summary>
        public double RefLayerDepthMeters { get; }

        /// <summary>
        ///     Heading
        /// </summary>
        public double Heading { get; }

        /// <summary>
        ///     Heading reference in use, True/Magnetic T/M
        /// </summary>
        public Reference HeadingRefInUse { get; }

        /// <summary>
        ///     Speed reference, B : Bottom track, W: Water track, P : Positioning System
        /// </summary>
        public SpeedReference SpeedRefInUse { get; }
    }
}