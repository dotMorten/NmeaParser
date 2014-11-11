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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmeaParser
{
	public class NmeaFileDevice : BufferedStreamDevice
	{
#if NETFX_CORE
		Windows.Storage.IStorageFile m_filename;
#else
		string m_filename;
#endif
		int m_readSpeed;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="readSpeed">The time to wait between each line being read in milliseconds</param>
#if NETFX_CORE
		public NmeaFileDevice(Windows.Storage.IStorageFile filename, int readSpeed = 200) : base(readSpeed)
#else
		public NmeaFileDevice(string filename, int readSpeed = 200) : base(readSpeed)
#endif
		{
			m_filename = filename;
			m_readSpeed = readSpeed;
		}
		protected override Task<Stream> GetStreamAsync()
		{
#if NETFX_CORE
			return m_filename.OpenStreamForReadAsync();
#else
			var sr = System.IO.File.OpenRead(m_filename);
			return Task.FromResult<Stream>(sr);
#endif
		}
	}
}
