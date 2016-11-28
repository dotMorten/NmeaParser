//
// Copyright (c) 2016 Gloucester Software Ltd. Author: Stephen Kennedy
//
// Licensed under the Microsoft Public License (Ms-PL) (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://opensource.org/licenses/Ms-PL.html
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

namespace NmeaParser.Nmea
{
    /// <summary>
    /// Enumerates the Nmea message type
    /// </summary>
    public enum NmeaMessageClassType
    {
        UnknownMessage,
        Ptnlb,
        Ptnla,
        Pltit,
        Gprte,
        Gprmc,
        Gprmb,
        Gpgsv,
        Gpgst,
        Gpgsa,
        Gpgll,
        Gpgga,
        Gpbod,
        Pgrmz,
        Pgrme
    }
}