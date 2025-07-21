/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Text;

namespace OneScript.DebugProtocol.TcpServer
{
    internal class BinaryFormatWriter : IDisposable
    {
        private readonly BinaryWriter _writer;
        
        public BinaryFormatWriter(Stream target)
        {
            _writer = new BinaryWriter(target, Encoding.UTF8, leaveOpen: true);
        }

        public void WriteHeader(int rootId, int headerId)
        {
            _writer.Write((byte)0);
            _writer.Write(rootId);
            _writer.Write(headerId);
            _writer.Write(1);
            _writer.Write(0);
        }

        public void WriteStringRecord(int objectId, string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            if (bytes.Length > 127)
                throw new ArgumentOutOfRangeException(nameof(data), "Length encoding not supported for strings more than 127 bytes");
            
            _writer.Write((byte)6);
            _writer.Write(objectId);
            _writer.Write((byte)bytes.Length);
            _writer.Write(bytes, 0, bytes.Length);
        }

        public void WriteEnd()
        {
            _writer.Write((byte)11);
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            _writer.Flush();
            _writer.Dispose();
        }
    }
}