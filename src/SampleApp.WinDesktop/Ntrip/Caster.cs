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
using System.Net;

namespace NmeaParser.Gnss.Ntrip
{
    public class Caster : NtripSource
    {
        internal Caster (string[] d)
        {
            var a = d[1].Split(':');
            Address = IPAddress.Parse(a[0]);
            Port = int.Parse(a[1]);
            Identifier = d[3];
            Operator = d[4];
            SupportsNmea = d[5] == "1";
            CountryCode = d[6];
            Latitude = double.Parse(d[7], CultureInfo.InvariantCulture);
            Longitude = double.Parse(d[8], CultureInfo.InvariantCulture);
            FallbackAddress = IPAddress.Parse(d[9]);
        }

        public Caster(IPAddress address, int port, string identifier, string _operator, bool supportsNmea, string countryCode, double latitude, double longitude, IPAddress fallbackkAddress)
        {
            Address = address;
            Port = port;
            Identifier = identifier;
            Operator = _operator;
            SupportsNmea = supportsNmea;
            CountryCode = countryCode;
            Latitude = latitude;
            Longitude = longitude;
            FallbackAddress = fallbackkAddress;
        }

        public IPAddress Address { get; }
        public int Port { get; }
        public string Identifier { get; }
        public string Operator { get; }
        public bool SupportsNmea { get; }
        public string CountryCode { get; }
        public double Latitude { get; }
        public double Longitude { get; }
        public IPAddress FallbackAddress { get; }
    }
}
