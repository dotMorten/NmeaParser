﻿//  *******************************************************************************
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
    public abstract class NmeaMessage
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
                        throw new InvalidOperationException($"Message type {nmeaType} declared in {typeInfo.FullName} is already registered by {messageTypes[nmeaType].DeclaringType.FullName}");
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
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// Invalid nmea message: Missing starting character '$'
        /// or checksum failure
        /// </exception>
        public static NmeaMessage Parse(string message, IMultiSentenceMessage? previousSentence = null)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            int checksum = -1;
            if (message[0] != '$')
                throw new ArgumentException("Invalid nmea message: Missing starting character '$'");
            var idx = message.IndexOf('*');
            if (idx >= 0)
            {
                checksum = Convert.ToInt32(message.Substring(idx + 1), 16);
                message = message.Substring(0, message.IndexOf('*'));
            }
            if (checksum > -1)
            {
                int checksumTest = 0;
                for (int i = 1; i < message.Length; i++)
                {
                    checksumTest ^= Convert.ToByte(message[i]);
                }
                if (checksum != checksumTest)
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid nmea message: Checksum failure. Got {0:X2}, Expected {1:X2}", checksum, checksumTest));
            }

            string[] parts = message.Split(new char[] { ',' });
            string MessageType = parts[0].Substring(1);
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
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
                    checksumTest ^= Convert.ToByte(message[i]);
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
    }
}
