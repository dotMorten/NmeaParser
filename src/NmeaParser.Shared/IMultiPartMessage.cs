using System;
using System.Collections.Generic;
using System.Text;

namespace NmeaParser
{
	interface IMultiPartMessage : System.Collections.IEnumerable
	{
		/// <summary>
		/// Total number of messages of this type in this cycle
		/// </summary>
		int TotalMessages { get; }

		/// <summary>
		/// Message number
		/// </summary>
		int MessageNumber { get; }
	}
	interface IMultiPartMessage<T> : IMultiPartMessage, IEnumerable<T>
    {
	}
}
