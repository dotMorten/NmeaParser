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
	/// <summary>
	/// A file-based NMEA device reading from a NMEA log file.
	/// </summary>
	public class NmeaFileDevice : BufferedStreamDevice
	{
#if NETFX_CORE
		Windows.Storage.IStorageFile m_filename;
#else
		string m_filename;
#endif
	
		/// <summary>
		/// Initializes a new instance of the <see cref="NmeaFileDevice"/> class.
		/// </summary>
		/// <param name="fileName"></param>
#if NETFX_CORE
		public NmeaFileDevice(Windows.Storage.IStorageFile fileName) : this(fileName, 1000)
#else
		public NmeaFileDevice(string fileName) : this(fileName, 1000)
#endif
		{
			m_filename = fileName;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="NmeaFileDevice"/> class.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="readSpeed">The time to wait between each group of lines being read in milliseconds</param>
#if NETFX_CORE
		public NmeaFileDevice(Windows.Storage.IStorageFile fileName, int readSpeed)
			: base(readSpeed)
#else
		public NmeaFileDevice(string fileName, int readSpeed) : base(readSpeed)
#endif
		{
			m_filename = fileName;
		}

#if !NETFX_CORE
		/// <summary>
		/// Gets the name of the nmea file this device is using.
		/// </summary>
		public string FileName
		{
			get { return m_filename; }
		}
#endif

		/// <summary>
		/// Gets the stream to perform buffer reads on.
		/// </summary>
		/// <returns></returns>
#if !NETFX_CORE
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
#endif
		protected override Task<Stream> GetStreamAsync()
		{
#if NETFX_CORE
			return m_filename.OpenStreamForReadAsync();
#else
			return Task.FromResult<Stream>(System.IO.File.OpenRead(m_filename));
#endif
		}
	}
}
