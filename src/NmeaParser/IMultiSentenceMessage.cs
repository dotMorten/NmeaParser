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
using System.Text;

namespace NmeaParser
{
	/// <summary>
	/// Interface used for NMEA messages that span multiple sentences
	/// </summary>
	public interface IMultiSentenceMessage
	{
		/// <summary>
		/// Attempts to append one message to an existing one
		/// </summary>
		/// <remarks>
		/// This method should return false if the message being appended isn't the next message in line, and various indicators show this is a different message than the previous one. It should also return false if you append to a message that didn't start with the first message.
		/// </remarks>
		/// <param name="messageType"></param>
		/// <param name="values"></param>
		/// <returns><c>True</c> is the message was successfully appended, <c>False</c> is the message couldn't be appended.</returns>
		bool TryAppend(string messageType, string[] values);

		/// <summary>
		/// Gets a value indicating whether this message is fully loaded from all its sentences
		/// </summary>
		bool IsComplete { get; }
	}
}
