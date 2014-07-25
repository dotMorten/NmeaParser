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
