﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.IO;

namespace VSCode.DebugAdapter
{
    static class SessionLog
    {
        private static StreamWriter _log;
        private static object lockObj = new object();

        private static string _path;
        [Conditional("DEBUG")]
        public static void Open(string path)
        {
            _path = path;
            _log = new StreamWriter(path);
            _log.AutoFlush = true;
            _log.WriteLine("started: " + DateTime.Now);
        }

        public static void WriteLine(string text)
        {
#if DEBUG
            lock (lockObj)
            {
                if (_log == null)
                {
                    _log = new StreamWriter(_path, true);
                    _log.AutoFlush = true;
                    _log.WriteLine("started: " + DateTime.Now);
                }
                _log.WriteLine(text);
            }
#endif
        }

        [Conditional("DEBUG")]
        public static void Close()
        {
            lock (lockObj)
            {
                if(_log == null)
                    return;
                
                _log.WriteLine("closed: " + DateTime.Now);
                _log.Dispose();
                _log = null;
            }
        }
    }
}
