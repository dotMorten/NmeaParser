using NmeaParser.Nmea;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NmeaParser
{
    /// <summary>
    /// Aggregates a group of messages, and raises an event once all messages has completed transmission
    /// </summary>
    public class MessageAggregator
    {
        private readonly Dictionary<string, Nmea.NmeaMessage> _messages = new Dictionary<string, Nmea.NmeaMessage>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageAggregator"/> class.
        /// </summary>
        /// <param name="device"></param>
        public MessageAggregator(NmeaDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            Device = device;
            Device.MessageReceived += Device_MessageReceived;
        }

        private void Device_MessageReceived(object sender, NmeaMessageReceivedEventArgs e)
        {
            if(_messages.ContainsKey(e.Message.MessageType))
            {
                var messages = _messages.Values.ToArray();
                _messages.Clear();
                OnMessagesReceived(messages);
                MessagesReceived?.Invoke(this, messages);
            }
            _messages.Add(e.Message.MessageType, e.Message);
        }

        /// <summary>
        /// Called when a group of messages have been received.
        /// </summary>
        protected virtual void OnMessagesReceived(NmeaMessage[] messages)
        {
        }

        /// <summary>
        /// Raised when a group of messages have been received.
        /// </summary>
        public event EventHandler<Nmea.NmeaMessage[]> MessagesReceived;

        /// <summary>
        /// The device that's being listened to.
        /// </summary>
        public NmeaDevice Device { get; }
    }
}
