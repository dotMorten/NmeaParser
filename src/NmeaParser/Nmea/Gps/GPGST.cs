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
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser.Nmea.Gps
{
	/// <summary>
	/// Position error statistics
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpgst")]
	[NmeaMessageType("GPGST")]
	public class Gpgst : NmeaMessage
	{
		/// <summary>
		/// Called when the message is being loaded.
		/// </summary>
		/// <param name="message">The NMEA message values.</param>
		protected override void OnLoadMessage(string[] message)
		{
			if (message == null || message.Length < 8)
				throw new ArgumentException("Invalid GPGST", "message");
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
