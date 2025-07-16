/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;

namespace OneScript.DebugProtocol.TcpServer
{
    public static class FormatReconcileUtils
    {
        /// <summary>
        /// Массив байт, который вызовет SerializationException при чтении его через BinaryFormatter
        /// И при этом оставит поток пустым, для следующего валидного сообщения
        /// <see>[MS-NRBF] .NET Remoting: Binary Format Data Structure</see>
        /// </summary>
        public static readonly byte[] FORMAT_RECONCILE_MAGIC = 
        {
            0x11, // RecordTypeEnum
            0,0,0,0, // RootId
            0,0,0,0, // HeaderId
            0x00, 0x0A, 0x00, 0x0A, // Version Major
            0xAA, 0xAA, 0xAA, 0xAA  // Version Minor,
        };

        /// <summary>
        /// Ответ на запрос формата
        /// </summary>
        public static readonly byte[] FORMAT_RECONCILE_RESPONSE_PREFIX =
        {
            0x1C,
            0x1C,
            0x1C,
            0x1C
        };
        
        public static readonly TimeSpan FORMAT_RECONCILE_TIMEOUT = TimeSpan.FromMilliseconds(500);

        public static bool CheckReconcilePrefix(byte[] data)
        {
            for (int i = 0; i < FORMAT_RECONCILE_RESPONSE_PREFIX.Length; i++)
            {
                if (data[i] != FORMAT_RECONCILE_RESPONSE_PREFIX[i]) 
                    return false;
            }

            return true;
        }

        public static bool CheckReconcileRequest(byte[] data)
        {
            for (int i = 0; i < FORMAT_RECONCILE_MAGIC.Length; i++)
            {
                if (data[i] != FORMAT_RECONCILE_MAGIC[i]) 
                    return false;
            }

            return true;
        }

        public static int EncodeFormatMarker(short transport, short dataVersion)
        {
            var marker = transport << 16;
            return marker | dataVersion;
        }
        
        public static (int, int) DecodeFormatMarker(int marker)
        {
            var transport = marker >> 16;
            var dataVersion = marker & 0x0000FFFF;

            return (transport, dataVersion);
        }
    }
}
