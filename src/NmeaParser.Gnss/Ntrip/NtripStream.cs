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

using System.Globalization;

namespace NmeaParser.Gnss.Ntrip
{
    public class NtripStream : NtripSource
    {
        internal NtripStream(string[] d)
        {
            Mountpoint = d[1];
            Identifier = d[2];
            Format = d[3];
            FormatDetails = d[4];
            Carrier = (Carrier)int.Parse(d[5]);
            Network = d[7];
            CountryCode = d[8];
            Latitude = double.Parse(d[9], CultureInfo.InvariantCulture);
            Longitude = double.Parse(d[10], CultureInfo.InvariantCulture);
            SupportsNmea = d[11] == "1";
        }

        public string Mountpoint { get; }
        public string Identifier { get; }
        public string Format { get; }
        public string FormatDetails { get; }
        public Carrier Carrier { get; }
        public string Network { get; }
        public string CountryCode { get; }
        public double Latitude { get; }
        public double Longitude { get; }
        public bool SupportsNmea { get; }
    }
}
