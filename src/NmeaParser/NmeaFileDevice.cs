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
		private Windows.Storage.IStorageFile m_storageFile;
#endif
        private string m_filename;


        /// <summary>
        /// Initializes a new instance of the <see cref="NmeaFileDevice"/> class.
        /// </summary>
        /// <param name="fileName"></param>
		public NmeaFileDevice(string fileName) : this(fileName, 1000)
        {
        }

#if NETFX_CORE
        /// <summary>
        /// Initializes a new instance of the <see cref="NmeaFileDevice"/> class.
        /// </summary>
        /// <param name="fileName"></param>
        public NmeaFileDevice(Windows.Storage.IStorageFile fileName) : this(fileName, 1000)
		{
		}
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="NmeaFileDevice"/> class.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="readSpeed">The time to wait between each group of lines being read in milliseconds</param>
		public NmeaFileDevice(string fileName, int readSpeed) : base(readSpeed)
		{
			m_filename = fileName;
		}

#if NETFX_CORE
        /// <summary>
        /// Initializes a new instance of the <see cref="NmeaFileDevice"/> class.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="readSpeed">The time to wait between each group of lines being read in milliseconds</param>
        public NmeaFileDevice(Windows.Storage.IStorageFile fileName, int readSpeed)
            : base(readSpeed)
        {
            m_storageFile = fileName;
        }
#endif

		/// <summary>
		/// Gets the name of the nmea file this device is using.
		/// </summary>
		public string FileName
		{
			get
            {
#if NETFX_CORE
                if (m_storageFile != null)
                    return m_storageFile.Path;
#endif
                return m_filename;
            }
		}

        /// <summary>
        /// Gets the stream to perform buffer reads on.
        /// </summary>
        /// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override Task<Stream> GetStreamAsync()
		{

#if NETFX_CORE
            if (m_storageFile != null)
                return m_storageFile.OpenStreamForReadAsync();
#endif
#if WINDOWS_STORE
            return Windows.Storage.StorageFile.GetFileFromPathAsync(m_filename).AsTask().ContinueWith(f => { return f.Result.OpenStreamForReadAsync(); }).ContinueWith(t => { return t.Result.Result; });
#else
            return Task.FromResult<Stream>(System.IO.File.OpenRead(m_filename));
#endif
        }
	}
}
