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

namespace NmeaParser.Messages
{
    /// <summary>
    ///     Heading from True North
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         1.: Heading in degrees
    ///         2.: Indicates heading relative to True North
    ///     </para>
    ///     <para>
    ///         Actual vessel heading in degrees True produced by any device or system producing true heading
    ///     </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "HeHdt")]
    [NmeaMessageType("--HDT")]
    public class Hdt : NmeaMessage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <exception cref="ArgumentException"></exception>
        public Hdt(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 2)
                throw new ArgumentException("Invalid Hdt", "message");

            HeadingInDeg = double.TryParse(message[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var tmp)
                ? tmp
                : double.NaN;
            HeadingRelToTrueNorth = message[1] == "T";
        }

        /// <summary>
        ///     Heading in degrees
        /// </summary>
        public double HeadingInDeg { get; }

        /// <summary>
        ///     Indicates heading relative to True North
        /// </summary>
        public bool HeadingRelToTrueNorth { get; }
    }
}