﻿//
// Copyright (c) 2014 Morten Nielsen
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser.Nmea
{
	/// <summary>
	/// Nmea message attribute type used on concrete <see cref="NmeaMessage"/> implementations.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class NmeaMessageTypeAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NmeaMessageTypeAttribute"/> class.
		/// </summary>
		/// <param name="nmeaType">The type.</param>
		public NmeaMessageTypeAttribute(string nmeaType)
		{
			NmeaType = nmeaType;
		}
		/// <summary>
		/// Gets or sets the NMEA message type.
		/// </summary>
		public string NmeaType { get; private set; }
	}

	/// <summary>
	/// NMEA Message base class.
	/// </summary>
	public abstract class NmeaMessage
	{
		/// <summary>
		/// Parses the specified NMEA message.
		/// </summary>
		/// <param name="message">The NMEA message string.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">
		/// Invalid nmea message: Missing starting character '$'
		/// or checksum failure
		/// </exception>
		public static NmeaMessage Parse(string message)
		{
			if (string.IsNullOrEmpty(message))
				throw new ArgumentNullException("message"); 
			
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
			if(messageTypes == null)
			{
				LoadResponseTypes();
			}
			NmeaMessage msg = null;
			if (messageTypes.ContainsKey(MessageType))
			{
				msg = (NmeaMessage)messageTypes[MessageType].Invoke(new object[] { });
			}
			else
			{
				msg = new UnknownMessage();
			}
			msg.MessageType = MessageType;
			msg.MessageParts = MessageParts;
			msg.OnLoadMessage(MessageParts);
			return msg;
		}

		private static void LoadResponseTypes()
		{
			messageTypes = new Dictionary<string, ConstructorInfo>();
			var typeinfo = typeof(NmeaMessage).GetTypeInfo();
			foreach (var subclass in typeinfo.Assembly.DefinedTypes.Where(t => t.IsSubclassOf(typeof(NmeaMessage))))
			{
				var attr = subclass.GetCustomAttribute<NmeaMessageTypeAttribute>(false);
				if (attr != null)
				{
					if (!subclass.IsAbstract)
					{
						foreach (var c in subclass.DeclaredConstructors)
						{
							var pinfo = c.GetParameters();
							if (pinfo.Length == 0)
							{
								messageTypes.Add(attr.NmeaType, c);
								break;
							}
						}
					}
				}
			}
		}

		private static Dictionary<string, ConstructorInfo> messageTypes;

		/// <summary>
		/// Gets the NMEA message parts.
		/// </summary>
		protected IReadOnlyList<string> MessageParts { get; private set; }

		/// <summary>
		/// Gets the NMEA type id for the message.
		/// </summary>
		public string MessageType { get; private set; }

		/// <summary>
		/// Called when the message is being loaded.
		/// </summary>
		/// <param name="message">The NMEA message values.</param>
		/// <remarks>
		/// Implement this method to create a custom NMEA message.
		/// </remarks>
		protected virtual void OnLoadMessage(string[] message) { MessageParts = message; }

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
		public byte Checksum
		{
			get
			{
				int checksumTest = 0;
				for (int j = -1; j < MessageParts.Count; j++)
				{
					string message = j < 0 ? MessageType : MessageParts[j];
					if (j >= 0)
						checksumTest ^= 0x2C; //Comma separator
					for (int i = 0; i < message.Length; i++)
					{
						checksumTest ^= Convert.ToByte(message[i]);
					}
				}
				return Convert.ToByte(checksumTest);
			}
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
			double result = double.NaN;
			if(value != null && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
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
