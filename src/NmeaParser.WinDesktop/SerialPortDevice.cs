using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace NmeaParser
{
	public class SerialPortDevice : NmeaDevice
	{
		private System.IO.Ports.SerialPort m_port;

		public SerialPortDevice(System.IO.Ports.SerialPort port)
		{
			m_port = port;
		}

		protected override Task<System.IO.Stream> OpenStreamAsync()
		{
			m_port.Open();
			return Task.FromResult<System.IO.Stream>(m_port.BaseStream);
		}

		protected override Task CloseStreamAsync(System.IO.Stream stream)
		{
			m_port.Close();
			return Task.FromResult(true);
		}
	}
}
