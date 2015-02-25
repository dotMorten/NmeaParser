using System;
using System.Collections.Generic;
using System.Text;

namespace NmeaParser.Nmea.Gps
{
    /// <summary>
    /// NMEA VTG message according to U-BLOX spec
    /// https://www.u-blox.com/images/downloads/Product_Docs/u-blox6_ReceiverDescriptionProtocolSpec_%28GPS.G6-SW-10018%29.pdf
    /// page 66
    /// </summary>
    [NmeaMessageType("GPVTG")]
    public class Gpvtg : NmeaMessage
    {
        /// <summary>
        /// Called when the message is being loaded.
        /// </summary>
        /// <param name="message">The NMEA message values.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnLoadMessage(string[] message)
        {
            TrueCourseOverGround = NmeaMessage.StringToDouble( message[0]);
            MagneticCourseOverGround = NmeaMessage.StringToDouble( message[2]);
            SpeedOverGroundKnots = NmeaMessage.StringToDouble( message[4]);
            SpeedOverGroundKph = NmeaMessage.StringToDouble(message[6]);

        }

        /// <summary>
        /// True course over ground in degrees
        /// </summary>
        public double TrueCourseOverGround { get; private set; }

        /// <summary>
        /// Magnetic course over ground in degrees
        /// </summary>
        public double MagneticCourseOverGround { get; private set; }

        /// <summary>
        /// Speed over ground in knots
        /// </summary>
        public double SpeedOverGroundKnots { get; private set; }

        /// <summary>
        /// Speed over ground in kilometers per hour
        /// </summary>
        public double SpeedOverGroundKph { get; private set; }
    }
}
