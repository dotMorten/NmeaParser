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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace NmeaParser.Messages
{
    /// <summary>
    /// Nmea message attribute type used on concrete <see cref="NmeaMessage"/> implementations.
    /// </summary>
    /// <remarks>
    /// The 5-character <see cref="NmeaType"/> indicates which message the class is meant to parse.
    /// Set the first two characters to <c>--</c> to make the message talker-independent.
    /// </remarks>
    /// <seealso cref="NmeaMessage.RegisterAssembly(Assembly, bool)"/>
    /// <seealso cref="NmeaMessage.RegisterNmeaMessage(TypeInfo, string, bool)"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class NmeaMessageTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NmeaMessageTypeAttribute"/> class.
        /// </summary>
        /// <param name="nmeaType">The 5-character NMEA type name, for instance <c>GPRMC</c>, or <c>--RMC</c> to make it apply to all talkers.</param>
        public NmeaMessageTypeAttribute(string nmeaType)
        {
            NmeaType = nmeaType;
        }
        /// <summary>
        /// Gets the NMEA message type name.
        /// </summary>
        /// <remarks>
        /// If the type name starts with <c>--</c>, this message can apply to all talker types.
        /// </remarks>
        public string NmeaType { get; private set; }
    }

    /// <summary>
    /// NMEA Message base class.
    /// </summary>
    public abstract class NmeaMessage : IEquatable<NmeaMessage>
    {
        private readonly static Dictionary<string, ConstructorInfo> messageTypes;

        /// <summary>
        /// Initializes an instance of the NMEA message
        /// </summary>
        /// <param name="messageType">Type</param>
        /// <param name="messageParts">Message values</param>
        protected NmeaMessage(string messageType, string[] messageParts)
        {
            MessageType = messageType;
            MessageParts = messageParts;
            Timestamp = System.Diagnostics.Stopwatch.GetTimestamp() * 1000d / System.Diagnostics.Stopwatch.Frequency;
        }

        static NmeaMessage()
        {
            messageTypes = new Dictionary<string, ConstructorInfo>();
            var typeinfo = typeof(NmeaMessage).GetTypeInfo();
            RegisterAssembly(typeinfo.Assembly);
        }

        /// <summary>
        /// Registers messages from a different assembly
        /// </summary>
        /// <remarks>
        /// The custom message MUST have a constructor taking a <c>string</c> as first parameter (message type name) and a <c>string[]</c> (message parts) as the second.
        /// In addition the class must have the <see cref="NmeaMessageTypeAttribute" /> defind on the class.
        /// </remarks>
        /// <param name="assembly">The assembly to load custom message types from</param>
        /// <param name="replace">Set to <c>true</c> if you want to replace already registered type. Otherwise this method will throw.</param>
        /// <returns>Number of message types found.</returns>
        public static int RegisterAssembly(Assembly assembly, bool replace = false)
        {
            int count = 0;
            foreach (var subclass in assembly.DefinedTypes.Where(t => t.IsSubclassOf(typeof(NmeaMessage)) && !t.IsAbstract))
            {
                var attr = subclass.GetCustomAttribute<NmeaMessageTypeAttribute>(false);
                if (attr != null)
                {
                    RegisterNmeaMessage(subclass, attr.NmeaType, replace);
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Registers a specific NMEA Message type
        /// </summary>
        /// <param name="typeInfo">TypeInfo for the class being registered</param>
        /// <param name="nmeaType">The 5-character NMEA Type name (eg <c>GPGLL</c>). If <c>null</c>, it'll expect the <see cref="NmeaMessageTypeAttribute" /> to be declared on the class. </param>
        /// <param name="replace">Set to <c>true</c> if you want to replace already registered type. Otherwise this method will throw.</param>
        public static void RegisterNmeaMessage(TypeInfo typeInfo, string nmeaType = "", bool replace = false)
        {
            if (string.IsNullOrEmpty(nmeaType))
            {
                var attr = typeInfo.GetCustomAttribute<NmeaMessageTypeAttribute>(false);
                if (attr == null)
                    throw new ArgumentException("Message does not have a NmeaMessageTypeAttribute and no type name was specified.");
                nmeaType = attr.NmeaType;
                if (string.IsNullOrEmpty(nmeaType))
                {
                    throw new ArgumentException("No NmeaType declared on the NmeaMessageTypeAttribute.");
                }
            }
            foreach (var c in typeInfo.DeclaredConstructors)
            {
                var pinfo = c.GetParameters();
                if (pinfo.Length == 2 && pinfo[0].ParameterType == typeof(string) && pinfo[1].ParameterType == typeof(string[]))
                {
                    if (!replace && messageTypes.ContainsKey(nmeaType))
                        throw new InvalidOperationException($"Message type {nmeaType} declared in {typeInfo.FullName} is already registered by {messageTypes[nmeaType].DeclaringType?.FullName}");
                    messageTypes[nmeaType] = c;
                    return;
                }
            }
            throw new ArgumentException("Type does not have a constructor with parameters (string,string[])");
        }

        /// <summary>
        /// Parses the specified NMEA message.
        /// </summary>
        /// <param name="message">The NMEA message string.</param>
        /// <param name="previousSentence">The previously received message (only used if parsing multi-sentence messages)</param>
        /// <param name="ignoreChecksum">If <c>true</c> ignores the checksum completely, if <c>false</c> validates the checksum if present.</param>
        /// <returns>The nmea message that was parsed.</returns>
        /// <exception cref="System.ArgumentException">
        /// Invalid nmea message: Missing starting character '$'
        /// or checksum failure
        /// </exception>
        public static NmeaMessage Parse(string message, IMultiSentenceMessage? previousSentence = null, bool ignoreChecksum = false)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            int checksum = -1;
            if (message[0] != '$')
                throw new ArgumentException("Invalid NMEA message: Missing starting character '$'");
            var idx = message.IndexOf('*');
            if (idx >= 0)
            {
                if (message.Length > idx + 1)
                {
                    if (int.TryParse(message.Substring(idx + 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int c))
                        checksum = c;
                    else
                        throw new ArgumentException("Invalid checksum string");
                }
                message = message.Substring(0, message.IndexOf('*'));
            }
            if (!ignoreChecksum && checksum > -1)
            {
                int checksumTest = 0;
                for (int i = 1; i < message.Length; i++)
                {
                    var c = message[i];
                    if (c < 0x20 || c > 0x7E)
                        throw new System.IO.InvalidDataException("NMEA Message contains invalid characters");
                    checksumTest ^= Convert.ToByte(c);
                }
                if (checksum != checksumTest)
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid NMEA message: Checksum failure. Got {0:X2}, Expected {1:X2}", checksum, checksumTest));
            }
            else
            {
                for (int i = 1; i < message.Length; i++)
                {
                    if (message[i] < 0x20 || message[i] > 0x7E)
                        throw new System.IO.InvalidDataException("NMEA Message contains invalid characters");
                }
            }

            string[] parts = message.Split(new char[] { ',' });
            string MessageType = parts[0].Substring(1);
            if (MessageType == "PTNL") {
                // PTNL is parent to e.g. AVR, GGK etc.
                MessageType = parts[1];
                parts = parts.Skip(1).ToArray();
            }
            if (MessageType == string.Empty)
                throw new ArgumentException("Missing NMEA Message Type");
            string[] MessageParts = parts.Skip(1).ToArray();
            if(previousSentence is NmeaMessage pmsg && pmsg.MessageType.Substring(2) == MessageType.Substring(2))
            {
                if (previousSentence.TryAppend(MessageType, MessageParts))
                {
                    return pmsg;
                }
            }
            if (messageTypes.ContainsKey(MessageType))
            {
                return (NmeaMessage)messageTypes[MessageType].Invoke(new object[] { MessageType, MessageParts });
            }
            else if (messageTypes.ContainsKey("--" + MessageType.Substring(2)))
            {
                return (NmeaMessage)messageTypes["--" + MessageType.Substring(2)].Invoke(new object[] { MessageType, MessageParts });
            }
            else
            {
                return new UnknownMessage(MessageType, MessageParts);
            }
        }

        /// <summary>
        /// Gets the NMEA message parts.
        /// </summary>
        protected IReadOnlyList<string> MessageParts { get; }

        /// <summary>
        /// Gets the NMEA type id for the message.
        /// </summary>
        /// <value>The 5 character string that identifies the message type</value>
        public string MessageType { get; }

        /// <summary>
        /// Gets the talker ID for this message (
        /// </summary>
        public virtual Talker TalkerId => TalkerHelper.GetTalker(MessageType);

        /// <summary>
        /// Gets a value indicating whether this message type is proprietary
        /// </summary>
        public bool IsProprietary => MessageType[0] == 'P'; //Appendix B

        /// <summary>
        /// Returns the original NMEA string that represents this message.
        /// </summary>
        /// <returns>An original NMEA string that represents this message.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "${0},{1}*{2:X2}", MessageType, string.Join(",", MessageParts), Checksum);
        }

        /// <summary>
        /// Gets the checksum value of the message.
        /// </summary>
        public byte Checksum => GetChecksum(MessageType, MessageParts);

        internal static byte GetChecksum(string messageType, IReadOnlyList<string> messageParts)
        {
            int checksumTest = 0;
            for (int j = -1; j < messageParts.Count; j++)
            {
                string message = j < 0 ? messageType : messageParts[j];
                if (j >= 0)
                    checksumTest ^= 0x2C; //Comma separator
                for (int i = 0; i < message.Length; i++)
                {
                    var c = message[i];
                    if (c < 256)
                        checksumTest ^= Convert.ToByte(c);
                }
            }
            return Convert.ToByte(checksumTest);
        }

        internal static double StringToLatitude(string value, string ns)
        {
            if (value == null || value.Length < 3)
                return double.NaN;
            double latitude = int.Parse(value.Substring(0, 2), CultureInfo.InvariantCulture) + double.Parse(value.Substring(2), CultureInfo.InvariantCulture) / 60;
            if (ns == "S")
                latitude *= -1;
            return latitude;
        }

        internal static double StringToLongitude(string value, string ew)
        {
            if (value == null || value.Length < 4)
                return double.NaN;
            double longitude = int.Parse(value.Substring(0, 3), CultureInfo.InvariantCulture) + double.Parse(value.Substring(3), CultureInfo.InvariantCulture) / 60;
            if (ew == "W")
                longitude *= -1;
            return longitude;
        }

        internal static double StringToDouble(string value)
        {
            if(value != null && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return double.NaN;
        }
        internal static TimeSpan StringToTimeSpan(string value)
        {
            if (value != null && value.Length >= 6)
            {
                return new TimeSpan(int.Parse(value.Substring(0, 2), CultureInfo.InvariantCulture),
                                   int.Parse(value.Substring(2, 2), CultureInfo.InvariantCulture), 0)
                                   .Add(TimeSpan.FromSeconds(double.Parse(value.Substring(4), CultureInfo.InvariantCulture)));
            }
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Parse from mmddyy + hhmmss.ss to DateTime (used in e.g. Ggk)
        /// https://receiverhelp.trimble.com/alloy-gnss/en-us/NMEA-0183messages_PTNL_GGK.html
        /// </summary>
        internal static DateTime? StringsToUtcDateTime(string dateField, string timeField)
        {
            if (string.IsNullOrWhiteSpace(dateField) || string.IsNullOrWhiteSpace(timeField))
                return null;

            // 2. Parse date (mmddyy)
            //    - Typically the year is 2 digits; you must decide how to handle the century.
            //    - For example, if year < 80 => 20xx; else => 19xx
            if (dateField.Length < 6) return null;
            if (!int.TryParse(dateField.Substring(0, 2), out int month)) return null;     // mm
            if (!int.TryParse(dateField.Substring(2, 2), out int day)) return null;       // dd
            if (!int.TryParse(dateField.Substring(4, 2), out int year)) return null;      // yy

            int fullYear = (year < 80) ? (2000 + year) : (1900 + year);

            // 3. Parse time (hhmmss.ss)
            //    - The fractional seconds might be optional or might have variable length.
            //    - Parse hours, minutes, and then the part after the decimal.
            //    - Time can be "hhmmss", or "hhmmss.sss", etc.
            // Example: "102939.00" => hh=10, mm=29, ss=39, fraction=0
            if (timeField.Length < 6) return null;
            if (!int.TryParse(timeField.Substring(0, 2), out int hours)) return null;     // hh
            if (!int.TryParse(timeField.Substring(2, 2), out int minutes)) return null;   // mm

            // The seconds part can have a decimal fraction
            // So we split around the decimal point:
            double secondsDouble;
            if (timeField.Length > 4)
            {
                string secString = timeField.Substring(4);  // "39.00 in example above"
                if (!double.TryParse(secString,
                                     System.Globalization.NumberStyles.Float,
                                     System.Globalization.CultureInfo.InvariantCulture,
                                     out secondsDouble))
                {
                    return null;
                }
            }
            else
            {
                secondsDouble = 0.0;
            }
            int seconds = (int)secondsDouble;
            int milliseconds = (int)((secondsDouble - seconds) * 1000.0);

            // 4. Construct the UTC DateTime
            try
            {
                var result = new DateTime(fullYear, month, day, hours, minutes, seconds, milliseconds, DateTimeKind.Utc);
                return result;
            }
            catch
            {
                // If the date/time is invalid (e.g. month=13, day=32), we return null or handle differently
                return null;
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(NmeaMessage? other)
        {
            if (other is null) return false;
            if (other.MessageType != MessageType)
                return false;
            if (other.MessageParts.Count != MessageParts.Count)
                return false;
            for (int i = 0; i < MessageParts.Count; i++)
            {
                if (other.MessageParts[i] != MessageParts[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets a relative timestamp in milliseconds indicating the time the message was created.
        /// </summary>
        /// <remarks>
        /// This value is deduced from <c>System.Diagnostics.Stopwatch.GetTimestamp() * 1000d / System.Diagnostics.Stopwatch.Frequency</c>.
        /// You can use it to calculate the age of the message in milliseconds by calculating the difference between the timestamp and the above expression
        /// </remarks>
        public double Timestamp { get; }
    }
}
