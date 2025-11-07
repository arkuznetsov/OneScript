/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Diagnostics;
using System.Threading;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;
using OneScript.DebugServices.Internal;
using ScriptEngine.Machine.Debugger;

namespace OneScript.DebugServices
{
    /// <summary>
    /// Простой отладчик, поддерживающий одновременно только одну сессию отладки.
    /// </summary>
    public class DefaultDebugger : IDebugger
    {
        // NB! должен быть согласован с перечислением TransportProtocols в адаптере
        private const short JSON_FORMAT_MARKER = 2;
        // NB! должен быть согласован с файлом ProtocolVersions в адаптере
        private const short SUPPORTED_FORMAT_VERSION = 3;
        
        private readonly IDebugServer _transport;
        private IDebugSession _session;
        
        private ManualResetEventSlim _connectionEvent = new ManualResetEventSlim();

        public DefaultDebugger(IDebugServer transport)
        {
            _transport = transport;
        }

        public bool IsEnabled => true;
        public bool WaitForSession { get; set; }

        public void Start()
        {
            _transport.OnClientConnected += TransportOnOnClientConnected;
            _transport.Listen();
        }

        private void TransportOnOnClientConnected(object sender, IDebuggerClient debuggerClient)
        {
            if (_session is DebugSession)
            {
                // Если есть активная сессия, то не начинаем новую
                debuggerClient.Dispose();
            }

            var dataStream = debuggerClient.GetDataStream();
            if (FormatReconcileUtils.CheckReconcileRequest(dataStream))
            {
                // Да, это наш фейковый заголовок
                FormatReconcileUtils.WriteReconcileResponse(dataStream, JSON_FORMAT_MARKER, SUPPORTED_FORMAT_VERSION);
            }
            
            var session = new DebugSession(debuggerClient, WaitForSession);
            session.OnClose += OnSessionClose;
            
            _session = session;
            _connectionEvent.Set();
        }

        private void OnSessionClose()
        {
            if (_session is DebugSession dbgSession)
            {
                dbgSession.OnClose -= OnSessionClose;
                _session = null;
            }
        }

        public IDebugSession GetSession()
        {
            if (_session == null)
            {
                if (WaitForSession)
                {
                    _connectionEvent.Wait();
                    Debug.Assert(_session != null);
                }
                else
                {
                    _session = new NoOpDebugSession();
                }
            }
            
            return _session;
        }

        public void NotifyProcessExit(int exitCode)
        {
            _transport.OnClientConnected-=TransportOnOnClientConnected;
            _transport.Stop();

            _session?.Dispose();
        }
    }
}