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

namespace NmeaParser.Gnss.Ntrip
{
    /// <summary>
    /// Metadata on an NTRIP Data Stream
    /// </summary>
    public class NtripStream : NtripSource
    {
        internal NtripStream(string[] d)
        {
            Mountpoint = d[1];
            Identifier = d[2];
            Format = d[3];
            FormatDetails = d[4];
            if (int.TryParse(d[5], out int carrier))
             Carrier = (Carrier)carrier;
            else
            {

            }
            Network = d[7];
            CountryCode = d[8];
            Latitude = double.Parse(d[9], CultureInfo.InvariantCulture);
            Longitude = double.Parse(d[10], CultureInfo.InvariantCulture);
            SupportsNmea = d[11] == "1";
        }

        /// <summary>
        /// The mountpoint used with <see cref="Client.Connect(string)"/>
        /// </summary>
        public string Mountpoint { get; }
        
        /// <summary>
        /// Gets the unique identifier for the stream
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Gets the stream format
        /// </summary>
        public string Format { get; }

        /// <summary>
        /// Gets the details about the format
        /// </summary>
        public string FormatDetails { get; }

        /// <summary>
        /// Gets the wave carrier for the stream
        /// </summary>
        public Carrier Carrier { get; }
        
        /// <summary>
        /// Gets the network for the stream
        /// </summary>
        public string Network { get; }
        
        /// <summary>
        /// Gets the country code for where the stream originates
        /// </summary>
        public string CountryCode { get; }
        
        /// <summary>
        /// Gets the latitude location of the base station
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// Gets the longitude location of the base station
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Gets a value indicating whether the stream supports NMEA
        /// </summary>
        public bool SupportsNmea { get; }
    }
}
