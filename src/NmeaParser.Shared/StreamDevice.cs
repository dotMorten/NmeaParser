using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser
{
    public class StreamDevice : NmeaDevice
    {
		System.IO.Stream m_stream;
		public StreamDevice(Stream stream) : base()
		{
			m_stream = stream;
		}

		protected override Task<Stream> OpenStreamAsync()
		{
			return Task.FromResult(m_stream);
		}

		protected override Task CloseStreamAsync(System.IO.Stream stream)
		{
			return Task.FromResult(true); //do nothing
		}

		protected override void Dispose(bool force)
		{
			if (m_stream != null)
				m_stream.Dispose();
			m_stream = null;
		}
    }
}
