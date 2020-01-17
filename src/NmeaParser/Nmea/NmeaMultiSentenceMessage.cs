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

namespace NmeaParser.Nmea
{
    /// <summary>
    /// Base class for easily creating message that are spread across multiple sentences
    /// </summary>
    public abstract class NmeaMultiSentenceMessage : NmeaMessage, IMultiSentenceMessage
    {
        private int lastMessageNumber = 0;
        private int totalMessages;
        private readonly int firstMessageNumber;
        private readonly List<string[]> messages = new List<string[]>();
        private readonly bool initialized;

        /// <summary>
        /// Initializes an instance of the <see cref="NmeaMultiSentenceMessage"/> class.
        /// </summary>
        /// <param name="messageType">Type</param>
        /// <param name="messageParts">Message values</param>
        protected NmeaMultiSentenceMessage(string messageType, string[] messageParts) : base(messageType, messageParts)
        {
            totalMessages = int.Parse(messageParts[MessageCountIndex], CultureInfo.InvariantCulture);
            firstMessageNumber = int.Parse(messageParts[MessageNumberIndex], CultureInfo.InvariantCulture);
            talkerId = base.TalkerId;
            if (!((IMultiSentenceMessage)this).TryAppend(messageType, messageParts))
                throw new ArgumentException("Failed to parse message");
            initialized = true;
        }

        /// <summary>
        /// Gets the index in the <see cref="NmeaMessage.MessageParts"/> where the total count of messages is listed.
        /// </summary>
        protected virtual int MessageCountIndex { get; } = 0;

        /// <summary>
        /// Gets the index in the <see cref="NmeaMessage.MessageParts"/> where the message number is listed.
        /// </summary>
        protected virtual int MessageNumberIndex { get; } = 1;

        bool IMultiSentenceMessage.IsComplete => firstMessageNumber == 1 && lastMessageNumber == totalMessages;

        /// <inheritdoc />
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var msg in messages)
            {
                if (sb.Length > 0)
                    sb.Append("\r\n");
                sb.AppendFormat(CultureInfo.InvariantCulture, "${0},{1}*{2:X2}", MessageType, string.Join(",", msg), Checksum);
            }
            return sb.ToString();
        }

        bool IMultiSentenceMessage.TryAppend(string messageType, string[] message)
        {
            if (message == null || message.Length < Math.Max(MessageCountIndex, MessageNumberIndex))
                throw new ArgumentException("Invalid message", "message");


            int msgCount = int.Parse(message[0], CultureInfo.InvariantCulture);
            int msgNumber = int.Parse(message[1], CultureInfo.InvariantCulture);

            if (initialized)
            {
                if (firstMessageNumber != 1) //We can only append to message who has message number 1
                    return false;
                if (msgCount != totalMessages || msgNumber != lastMessageNumber + 1)
                    return false; // Messages do not match
            }

            var talker = TalkerHelper.GetTalker(messageType);
            if (talkerId != talker)
                talkerId = Talker.Multiple;
            if (ParseSentences(talker, message))
            {
                lastMessageNumber = msgNumber;
                messages.Add(message);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Parses the messages or any message being appended. False should be returned if it's a message being appended doesn't appear to match what has already been loded.
        /// </summary>
        /// <param name="talkerType"></param>
        /// <param name="message"></param>
        /// <returns>True if the message could succesfully be appended.</returns>
        protected abstract bool ParseSentences(Talker talkerType, string[] message);

        private Talker talkerId;


        /// <inheritdoc />
        public override Talker TalkerId => talkerId;

    }
}
