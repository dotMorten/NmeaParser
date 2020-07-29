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
    /// Local geodetic datum and datum offsets from a reference datum.
    /// </summary>
    /// <remarks>
    /// <para>This sentence is used to define the datum to which a position location and geographic
    /// locations in subsequent sentences, is referenced. Latitude, longitude and altitude offsets
    /// from the reference datum, and the selection of reference datum, are also provided.</para>
	/// <para>
	/// The datum sentence should be transmitted immediately prior to every positional sentence (e.g., <c>GLL</c>, 
	/// <c>BWC</c>, <c>WPL</c>) that is referenced to a datum other than WGS84, which is the datum recommended by IMO.
	/// </para>
	/// <para>
	/// For all datums the DTM sentence should be transmitted prior to any datum change and periodically at
	/// intervals of not greater than 30 seconds.
	/// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dtm")]
    [NmeaMessageType("--DTM")]
    public class Dtm : NmeaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gll"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Dtm (string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 6)
                throw new ArgumentException("Invalid DTM", "message");
            LocalDatumCode = message[0];
            if (message[1].Length > 0)
                LocalDatumSubdivisionCode = message[1][0];
            LatitudeOffset = NmeaMessage.StringToDouble(message[2]);
            LongitudeOffset = NmeaMessage.StringToDouble(message[3]);
            AltitudeOffset = NmeaMessage.StringToDouble(message[4]);
            ReferenceDatumCode = message[5];
        }

        /// <summary>
        /// Local datum code
        /// </summary>
        /// <remarks>
        /// <para>Three character alpha code for local datum. If not one of the listed earth-centered datums, or <c>999</c>
        /// for user defined datum, use IHO datum code from International Hydrographic Organization Publication S-60
        /// Appendices B and C. String.Empty if unknown.</para>
        /// <para>
        /// Users should be aware that chart transformations based on IHO S60 parameters may result in significant
        /// positional errors when applied to chart data.
        /// </para>
		/// <para>
		/// Common known datum codes are:
		/// <table>
        ///   <tr>
        ///     <th>Code</th>
        ///     <th>Datum</th>
        ///   </tr>
        ///   <tr><td><c>W84</c></td><td>WGS 84</td></tr>
        ///   <tr><td><c>W72</c></td><td>WGS 72</td></tr>
        ///   <tr><td><c>S85</c></td><td>SGS 85</td></tr>
        ///   <tr><td><c>P90</c></td><td>PE 90</td></tr>
        ///   <tr><td><c>999</c></td><td>User Defined</td></tr>
        ///   <tr><td><c>Others</c></td><td>IHO Datum Code</td></tr>
        /// </table>
		/// </para>
        /// </remarks>
        public string LocalDatumCode { get; }

        /// <summary>
        /// Local datum subdivision code.
        /// </summary>
        /// <remarks>
        /// One character subdivision datum code when available or user defined reference character
        /// for user defined datums, null field otherwise. Subdivision character from IHO Publication S-60
        /// Appendices B and C.
        /// </remarks>
        public char? LocalDatumSubdivisionCode { get; }

        /// <summary>
        /// Latitude Offset, decimal degrees
        /// </summary>
        /// <remarks>
        /// Latitude and longitude offsets are positive numbers, the altitude offset may be negative. Offsets
		/// change with position; position in the local datum is offset from the position in the reference datum in the directions 
		/// indicated:
		/// <c>P_local_datum = P_ref_datum + offset</c>
        /// </remarks>
        public double LatitudeOffset { get; }

        /// <summary>
        /// Longitude Offset in minutes
        /// </summary>
        /// <remarks>
        /// Latitude and longitude offsets are positive numbers, the altitude offset may be negative. Offsets
		/// change with position; position in the local datum is offset from the position in the reference datum in the directions 
		/// indicated:
		/// <c>P_local_datum = P_ref_datum + offset</c>
        /// </remarks>
        public double LongitudeOffset { get; }

        /// <summary>
        /// Altitude Offset in minutes
        /// </summary>
        /// <remarks>
        /// Latitude and longitude offsets are positive numbers, the altitude offset may be negative. Offsets
		/// change with position; position in the local datum is offset from the position in the reference datum in the directions 
		/// indicated:
		/// <c>P_local_datum = P_ref_datum + offset</c>
        /// </remarks>
        public double AltitudeOffset { get; }

        /// <summary>
        /// Reference datum code
        /// </summary>        
        /// <remarks>
		/// <para>
		/// Common known datum codes are:
		/// <table>
        ///   <tr>
        ///     <th>Code</th>
        ///     <th>Datum</th>
        ///   </tr>
        ///   <tr><td><c>W84</c></td><td>WGS 84</td></tr>
        ///   <tr><td><c>W72</c></td><td>WGS 72</td></tr>
        ///   <tr><td><c>S85</c></td><td>SGS 85</td></tr>
        ///   <tr><td><c>P90</c></td><td>PE 90</td></tr>
        /// </table>
		/// </para>
        /// </remarks>
        public string ReferenceDatumCode { get; }
    }
}