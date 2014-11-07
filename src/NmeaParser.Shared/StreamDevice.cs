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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser
{
	/// <summary>
	/// Generic stream device
	/// </summary>
    public class StreamDevice : NmeaDevice
    {
		System.IO.Stream m_stream;
		public StreamDevice(Stream stream) : base()
		{
			m_stream = stream;
		}

		protected override Task<Stream> OpenStreamAsync()
		{
			return TaskEx.FromResult(m_stream);
		}

		protected override Task CloseStreamAsync(System.IO.Stream stream)
		{
			return TaskEx.FromResult(true); //do nothing
		}

		protected override void Dispose(bool force)
		{
			if (m_stream != null)
				m_stream.Dispose();
			m_stream = null;
		}
    }
}
