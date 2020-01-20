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
using System.Globalization;

namespace NmeaParser.Messages.Trimble
{
    /// <summary>
    /// Laser Range Tree Measurement
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ptnlb")]
    [NmeaMessageType("PTNLB")]
    public class Ptnlb : NmeaMessage
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Ptnlb"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Ptnlb(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 6)
                throw new ArgumentException("Invalid PTNLB", "message");

            TreeHeight = message[0];
            MeasuredTreeHeight = double.Parse(message[1], CultureInfo.InvariantCulture);
            MeasuredTreeHeightUnits = message[2][0];
            TreeDiameter = message[3];
            MeasuredTreeDiameter = double.Parse(message[4], CultureInfo.InvariantCulture);
            MeasuredTreeDiameterUnits = message[5][0];
        }

        /// <summary>
        /// Gets the height of the tree.
        /// </summary>
        public string TreeHeight { get; }

        /// <summary>
        /// Gets the message height of the tree.
        /// </summary>
        public double MeasuredTreeHeight { get; }

        /// <summary>
        /// Gets the units of the <see cref="MeasuredTreeHeight"/> value.
        /// </summary>
        public char MeasuredTreeHeightUnits { get; }

        /// <summary>
        /// Gets the tree diameter.
        /// </summary>
        public string TreeDiameter { get; }

        /// <summary>
        /// Gets the measured tree diameter.
        /// </summary>
        public double MeasuredTreeDiameter { get; }

        /// <summary>
        /// Gets the units of the <see cref="MeasuredTreeDiameter"/> value.
        /// </summary>
        public char MeasuredTreeDiameterUnits { get; }

        //more to do...

    }
}
