/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;

namespace OneScript.DebugServices.Internal
{
    internal class DelayedConnectionChannel : ICommunicationChannel
    {
        private readonly TcpListener _listener;
        private ICommunicationChannel _connectedChannel;

        // NB! должен быть согласован с перечислением TransportProtocols в адаптере
        private const short JSON_FORMAT_MARKER = 2;
        // NB! должен быть согласован с файлом ProtocolVersions в адаптере
        private const short SUPPORTED_FORMAT_VERSION = 3;
        
        private bool _reconciled;
        private readonly object _lock = new object();

        public DelayedConnectionChannel(TcpListener listener)
        {
            _listener = listener;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _listener?.Stop();
                _connectedChannel?.Dispose();
            }
        }

        public void Write(object data)
        {
            lock (_lock)
            {
                if (_connectedChannel == null)
                    throw new InvalidOperationException("No client connected");

                try
                {
                    _connectedChannel.Write(data);
                }
                catch (IOException)
                {
                    // Connection lost, reset for reconnection
                    ResetConnection();
                    throw;
                }
                catch (ObjectDisposedException)
                {
                    // Connection lost, reset for reconnection
                    ResetConnection();
                    throw;
                }
            }
        }

        public T Read<T>()
        {
            ReconcileFormat();
            try
            {
                return _connectedChannel.Read<T>();
            }
            catch (IOException)
            {
                // Connection lost, reset for reconnection
                lock (_lock)
                {
                    ResetConnection();
                }
                throw;
            }
            catch (ObjectDisposedException)
            {
                // Connection lost, reset for reconnection
                lock (_lock)
                {
                    ResetConnection();
                }
                throw;
            }
        }

        public object Read()
        {
            ReconcileFormat();
            try
            {
                return _connectedChannel.Read();
            }
            catch (IOException)
            {
                // Connection lost, reset for reconnection
                lock (_lock)
                {
                    ResetConnection();
                }
                throw;
            }
            catch (ObjectDisposedException)
            {
                // Connection lost, reset for reconnection
                lock (_lock)
                {
                    ResetConnection();
                }
                throw;
            }
        }

        private void ReconcileFormat()
        {
            lock (_lock)
            {
                if (_reconciled && _connectedChannel?.Connected == true)
                    return;

                // Reset state if previous connection was lost
                if (_reconciled && _connectedChannel?.Connected == false)
                {
                    ResetConnection();
                }

                _listener.Start();
                var tcpClient = _listener.AcceptTcpClient();
                _listener.Stop();

                var tcpStream = tcpClient.GetStream();

                if (FormatReconcileUtils.CheckReconcileRequest(tcpStream))
                {
                    // Да, это наш фейковый заголовок
                    FormatReconcileUtils.WriteReconcileResponse(tcpStream, JSON_FORMAT_MARKER, SUPPORTED_FORMAT_VERSION);
                }
                _reconciled = true;
                _connectedChannel = new JsonDtoChannel(tcpClient);
            }
        }

        private void ResetConnection()
        {
            _connectedChannel?.Dispose();
            _connectedChannel = null;
            _reconciled = false;
        }

        public bool Connected => _connectedChannel?.Connected ?? false;
    }
}