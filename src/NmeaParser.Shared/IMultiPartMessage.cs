using System;
using System.Collections.Generic;
using System.Text;

namespace NmeaParser
{
    interface IMultiPartMessage<T> : IEnumerable<T>
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
}
