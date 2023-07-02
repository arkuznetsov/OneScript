﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OneScript.StandardLibrary.Binary;
using OneScript.StandardLibrary.Text;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.Http
{
    class HttpRequestBodyBinary : IHttpRequestBody
    {
        private readonly MemoryStream _memoryStream = new MemoryStream();

        public HttpRequestBodyBinary()
        {
        }

        public HttpRequestBodyBinary(BinaryDataContext data)
        {
            data.CopyTo(_memoryStream);
        }

        public HttpRequestBodyBinary(string body, IValue encoding = null,
            ByteOrderMarkUsageEnum bomUsage = ByteOrderMarkUsageEnum.Auto)
        {
            var utfs = new List<string> {"utf-16", "utf-32"};
            var addBom = utfs.Contains(encoding?.AsString(), StringComparer.OrdinalIgnoreCase) &&
                         bomUsage == ByteOrderMarkUsageEnum.Auto || bomUsage == ByteOrderMarkUsageEnum.Use;

            var encoder = encoding == null ? new UTF8Encoding(addBom) : TextEncodingEnum.GetEncoding(encoding, addBom);

            var byteArray = encoder.GetBytes(body);
            _memoryStream.Write(byteArray, 0, byteArray.Length);
        }

        public IValue GetAsString()
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(_memoryStream);
            return ValueFactory.Create(reader.ReadToEnd());
        }

        public IValue GetAsBinary()
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            return new BinaryDataContext(_memoryStream);
        }

        public IValue GetAsFilename()
        {
            return ValueFactory.Create();
        }

        public Stream GetDataStream()
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            return _memoryStream;
        }

        public void Dispose()
        {
            _memoryStream.Close();
        }
    }
}