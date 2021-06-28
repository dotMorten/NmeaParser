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
    ///     Meteorological Composite.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         1.: Barometric pressure, inches of mercury, to the nearest 0.01 inch
    ///         2.: I = inches of mercury
    ///         3.: Barometric pressure, bars, to the nearest .001 bar
    ///         4.: B = bars
    ///         5.: Air temperature, degrees C, to the nearest 0.1 degree C
    ///         6.: C = degrees C
    ///         7.: Water temperature, degrees C (this field left blank by WeatherStation)
    ///         8.: C = degrees C(this field left blank by WeatherStation)
    ///         9.: Relative humidity, percent, to the nearest 0.1 percent
    ///         10.:Absolute humidity, percent(this field left blank by WeatherStation)
    ///         11.:Dew point, degrees C, to the nearest 0.1 degree C
    ///         12.:C = degrees C
    ///         13.:Wind direction, degrees True, to the nearest 0.1 degree
    ///         14.:T = true
    ///         15.:Wind direction, degrees Magnetic, to the nearest 0.1 degree
    ///         16.:M = magnetic
    ///         17.:Wind speed, knots, to the nearest 0.1 knot
    ///         18.:N = knots
    ///         19.:Wind speed, meters per second, to the nearest 0.1 m/s
    ///         20.:M = meters per second
    ///     </para>
    ///     <para>
    ///         Barometric pressure, air and water temperature, humidity, dew point and wind speed and direction relative to the surface of the earth.
    ///     </para>
    /// </remarks>
    ///
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "GpMda")][NmeaMessageType("--MDA")]
    public class Mda : NmeaMessage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Mda" /> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Mda(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 20)
                throw new ArgumentException("Invalid Mda", "message");

            BarometricPresI = double.TryParse(message[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var tmp)
                ? tmp
                : double.NaN;
            BarometricPresIUnit = message[1] == "I" ? Unit.Inc : Unit.NaN;

            BarometricPresBar = double.TryParse(message[2], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;
            BarometricPresBarUnit = message[3] == "B" ? Unit.Bar : Unit.NaN;

            AirTemp = double.TryParse(message[4], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;
            AirTempUnit = message[5] == "C" ? Unit.C : Unit.NaN;

            WaterTemp = double.TryParse(message[6], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;
            WaterTempUnit = message[7] == "C" ? Unit.C : Unit.NaN;

            RelativeHumidity = double.TryParse(message[8], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;
            AbsoluteHumidity = double.TryParse(message[9], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;

            DewPoint = double.TryParse(message[10], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;
            DewPointUnit = message[11] == "C" ? Unit.C : Unit.NaN;

            WindDirTrue = double.TryParse(message[12], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;
            WindDirTrueReference = message[13] == "T" ? Reference.True : Reference.NaN;

            WindDirMag = double.TryParse(message[14], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;
            WindDirMegReference = message[15] == "M" ? Reference.Magnetic : Reference.NaN;

            WindSpeedKnot = double.TryParse(message[16], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;
            WindSpeedKnotUnit = message[17] == "N" ? Unit.Knot : Unit.NaN;

            WindSpeedMsec = double.TryParse(message[18], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)
                ? tmp
                : double.NaN;
            WindSpeedMsecUnit = message[19] == "M" ? Unit.Msec : Unit.NaN;

        }

        /// <summary>
        ///     Barometric pressure, inches of mercury, to the nearest 0.01 inch
        /// </summary>
        public double BarometricPresI { get; }

        /// <summary>
        ///     Barometric pressure I = inches of mercury
        /// </summary>
        public Unit BarometricPresIUnit { get; }

        /// <summary>
        ///     Barometric pressure, bars, to the nearest .001 bar
        /// </summary>
        public double BarometricPresBar { get; }

        /// <summary>
        ///     Barometric pressure, bars, to the nearest .001 bar = bar
        /// </summary>
        public Unit BarometricPresBarUnit { get; }

        /// <summary>
        ///     Air temperature, degrees C, to the nearest 0.1 degree C
        /// </summary>
        public double AirTemp { get; }

        /// <summary>
        ///     Air temperature, degrees C, to the nearest 0.1 degree C C = degrees C
        /// </summary>
        public Unit AirTempUnit { get; }

        /// <summary>
        ///     Water temperature, degrees C (this field left blank by WeatherStation)
        /// </summary>
        public double WaterTemp { get; }

        /// <summary>
        ///    Water temperature, degrees C (this field left blank by WeatherStation) C = degrees C (this field left blank by WeatherStation)
        /// </summary>
        public Unit WaterTempUnit { get; }

        /// <summary>
        ///     Relative humidity, percent, to the nearest 0.1 percent
        /// </summary>
        public double RelativeHumidity { get; }

        /// <summary>
        ///     Absolute humidity, percent (this field left blank by WeatherStation)
        /// </summary>
        public double AbsoluteHumidity { get; }

        /// <summary>
        ///     Dew point, degrees C, to the nearest 0.1 degree C
        /// </summary>
        public double DewPoint { get; }

        /// <summary>
        ///     Dew point, degrees C, to the nearest 0.1 degree C C = degrees C
        /// </summary>
        public Unit DewPointUnit { get; }

        /// <summary>
        ///     Wind direction, degrees True, to the nearest 0.1 degree
        /// </summary>
        public double WindDirTrue { get; }

        /// <summary>
        ///      Wind direction, degrees True, to the nearest 0.1 degree Reference, R = Relative, T = True
        /// </summary>
        public Reference WindDirTrueReference { get; }

        /// <summary>
        ///     Wind direction, degrees Magnetic, to the nearest 0.1 degree
        /// </summary>
        public double WindDirMag { get; }

        /// <summary>
        ///      Wind direction, degrees True, to the nearest 0.1 degree Reference, R = Relative, T = True
        /// </summary>
        public Reference WindDirMegReference { get; }

        /// <summary>
        ///      Wind Speed N = knots
        /// </summary>
        public double WindSpeedKnot { get; }

        /// <summary>
        ///     Wind Speed Units, N = knots
        /// </summary>
        public Unit WindSpeedKnotUnit { get; }

        /// <summary>
        ///      Wind Speed M = meters per second
        /// </summary>
        public double WindSpeedMsec { get; }

        /// <summary>
        ///     Wind Speed Units, M = meters per second
        /// </summary>
        public Unit WindSpeedMsecUnit { get; }
    }
}

