using System;

namespace NmeaParser.Nmea.Gnss
{
    /// <summary>
    /// Position error statistics 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gngst")]
    [NmeaMessageType("GNGST")]
    public class Gngst : NmeaMessage
    {
        /// <summary>
        /// Called when the message is being loaded.
        /// </summary>
        /// <param name="message">The NMEA message values.</param>
        protected override void OnLoadMessage(string[] message)
        {
            if (message == null || message.Length < 8)
                throw new ArgumentException("Invalid GNGST", "message");
            FixTime = StringToTimeSpan(message[0]);
            Rms = NmeaMessage.StringToDouble(message[1]);
            SemiMajorError = NmeaMessage.StringToDouble(message[2]);
            SemiMinorError = NmeaMessage.StringToDouble(message[3]);
            ErrorOrientation = NmeaMessage.StringToDouble(message[4]);
            SigmaLatitudeError = NmeaMessage.StringToDouble(message[5]);
            SigmaLongitudeError = NmeaMessage.StringToDouble(message[6]);
            SigmaHeightError = NmeaMessage.StringToDouble(message[7]);
        }

        /// <summary>
	/// UTC of position fix
	/// </summary>
	public TimeSpan FixTime { get; private set; }

        /// <summary>
        /// RMS value of the pseudorange residuals; includes carrier phase residuals during periods of RTK (float) and RTK (fixed) processing
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rms")]
        public double Rms { get; private set; }

        /// <summary>
        /// Error ellipse semi-major axis 1 sigma error, in meters
        /// </summary>
        public double SemiMajorError { get; private set; }

        /// <summary>
        /// Error ellipse semi-minor axis 1 sigma error, in meters
        /// </summary>
        public double SemiMinorError { get; private set; }

        /// <summary>
        /// Error ellipse orientation, degrees from true north
        /// </summary>
        public double ErrorOrientation { get; private set; }

        /// <summary>
        /// Latitude 1 sigma error, in meters
        /// </summary>
        /// <remarks>
        /// The error expressed as one standard deviation.
        /// </remarks>
        public double SigmaLatitudeError { get; private set; }

        /// <summary >
        /// Longitude 1 sigma error, in meters
        /// </summary>
        /// <remarks>
        /// The error expressed as one standard deviation.
        /// </remarks>
        public double SigmaLongitudeError { get; private set; }

        /// <summary >
        /// Height 1 sigma error, in meters
        /// </summary>
        /// <remarks>
        /// The error expressed as one standard deviation.
        /// </remarks>
        public double SigmaHeightError { get; private set; }
    }
}
