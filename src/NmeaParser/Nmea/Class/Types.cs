namespace NmeaParser.Nmea.Class
{
    /// <summary>
    ///     Data reference in use
    /// </summary>
    public enum Reference
    {
        /// <summary>
        ///     True
        /// </summary>
        True,

        /// <summary>
        ///     Relative
        /// </summary>
        Relative,

        /// <summary>
        ///     Magnetic
        /// </summary>
        Magnetic,

        /// <summary>
        ///     No data available
        /// </summary>
        NaN
    }
    
    /// <summary>
    ///     Speed reference in use
    /// </summary>
    public enum SpeedReference
    {
        /// <summary>
        ///     Bottom track
        /// </summary>
        BottomTrack,

        /// <summary>
        ///     Wather track
        /// </summary>
        WatherTrack,

        /// <summary>
        ///     Positioning System
        /// </summary>
        PosSystem,

        /// <summary>
        ///     No data available
        /// </summary>
        NaN
    }

    /// <summary>
    ///     Data status
    /// </summary>
    public enum Validation
    {
        /// <summary>
        ///     Ok
        /// </summary>
        Ok,

        /// <summary>
        ///     Warning
        /// </summary>
        Warning,

        /// <summary>
        ///     Invalid
        /// </summary>
        Invalid,

        /// <summary>
        ///     No data available
        /// </summary>
        NaN
    }

    /// <summary>
    ///     Unit
    /// </summary>
    public enum Unit
    {
        /// <summary>
        ///     Km/h
        /// </summary>
        Kmh,

        /// <summary>
        ///     Meter
        /// </summary>
        M,

        /// <summary>
        ///     Meters per second
        /// </summary>
        Msec,

        /// <summary>
        ///     Miles
        /// </summary>
        Mile,

        /// <summary>
        ///     Knot
        /// </summary>
        Knot,

        /// <summary>
        ///     Inches of mercury
        /// </summary>
        Inc,

        /// <summary>
        ///     Barometric pressure, bars
        /// </summary>
        Bar,

        /// <summary>
        ///     Air temperature, degrees C
        /// </summary>
        C,

        /// <summary>
        ///     Degrees
        /// </summary>
        Degrees,

        /// <summary>
        ///     Newtons
        /// </summary>
        Newtons,

        /// <summary>
        ///     Percent
        /// </summary>
        Percent,

        /// <summary>
        ///     No data available
        /// </summary>
        NaN
    }

    /// <summary>
    ///     TransducerTypes
    /// </summary>
    public enum TransducerTypes
    {
        /// <summary>
        ///     Angular displacement
        /// </summary>
        AngularDisplacement,

        /// <summary>
        ///     Temperature
        /// </summary>
        Temperature,

        /// <summary>
        ///     Depth
        /// </summary>
        Depth,

        /// <summary>
        ///     Frequency
        /// </summary>
        Frequency,

        /// <summary>
        ///     Humidity
        /// </summary>
        Humidity,

        /// <summary>
        ///     Force
        /// </summary>
        Force,

        /// <summary>
        ///     Pressure
        /// </summary>
        Pressure,

        /// <summary>
        ///     Flow
        /// </summary>
        Flow,

        /// <summary>
        ///     No data available
        /// </summary>
        NaN
    }
}