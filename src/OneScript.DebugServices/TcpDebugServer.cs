/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;
using OneScript.DebugServices.Internal;
using ScriptEngine.Machine;

namespace OneScript.DebugServices
{
    public class TcpDebugServer : IDebugServer
    {
        private readonly int _port;
        private volatile bool _isListening;
        private TcpListener _listener;

        public TcpDebugServer(int port)
        {
            _port = port;
        }

        public enum LogLevel
        {
            Debug,
            Info,
            Error
        }
        
        public delegate void LogHandler(LogLevel level, string message);
        
        public event LogHandler OnLogEvent;

        // public IDebugController CreateDebugController()
        // {
        //     var listener = TcpListener.Create(_port);
        //     var channel = new DelayedConnectionChannel(listener);
        //     var ipcServer = new DefaultMessageServer<RpcCall>(channel)
        //     {
        //         ServerThreadName = "debug-server"
        //     };
        //     var callback = new TcpEventCallbackChannel(channel);
        //     var threadManager = new ThreadManager();
        //     var breakpoints = new DefaultBreakpointManager();
        //     var debuggerService = new DefaultDebugService(breakpoints, threadManager, new DefaultVariableVisualizer());
        //     var controller = new DefaultDebugController(ipcServer, debuggerService, callback, threadManager, breakpoints);
        //
        //     return controller;
        // }
        
        public void Listen()
        {
            if (_isListening)
                return;
            
            _listener = TcpListener.Create(_port);

            _listener.Start();
            _isListening = true;
            
            StartListenerThread();
        }

        private void StartListenerThread()
        {
            new Thread(() =>
            {
                OnLogEvent?.Invoke(LogLevel.Info, "Starting listener thread");
                while (_isListening)
                {
                    try
                    {
                        var client = _listener.AcceptTcpClient();
                        OnLogEvent?.Invoke(LogLevel.Debug, "Client connected");
                        OnClientConnected?.Invoke(this, new TcpDebuggerClient(client));
                    }
                    catch (ObjectDisposedException)
                    {
                        _isListening = false;
                        OnLogEvent?.Invoke(LogLevel.Debug, "Listener interrupted");
                        break;
                    }
                    catch (SocketException e)
                    {
                        OnLogEvent?.Invoke(LogLevel.Error, $"Socket exception: {e}");
                        if (OnListenException == null)
                        {
                            Stop();
                            break;
                        }
                        
                        var args = new ListenerErrorEventArgs(e);
                        try
                        {
                            OnLogEvent?.Invoke(LogLevel.Debug, $"Calling exception handler");
                            OnListenException?.Invoke(this, args);
                            if (args.StopServer)
                            {
                                Stop();
                            }
                        }
                        catch (Exception handlerException)
                        {
                            OnLogEvent?.Invoke(LogLevel.Error, $"Exception handler raised exception {handlerException}");
                            Stop();
                            throw;
                        }
                    }
                }
                OnLogEvent?.Invoke(LogLevel.Info, "Listener thread stopped");
            })
            {
                IsBackground = true,
                Name = "debug-server-listener",
            }.Start();
        }

        private class TcpDebuggerClient : IDebuggerClient
        {
            private TcpClient Client { get; }

            public TcpDebuggerClient(TcpClient client)
            {
                Client = client;
            }

            public void Dispose()
            {
                Client.Dispose();
            }

            public Stream GetDataStream()
            {
                return Client.GetStream();
            }

            public bool Connected => Client.Connected;
        }

        public void Stop()
        {
            OnLogEvent?.Invoke(LogLevel.Info, "Stopping listener");
            _isListening = false;
            try
            {
                _listener.Stop();
            }
            catch (Exception e)
            {
                // тут нечего делать
                OnLogEvent?.Invoke(LogLevel.Error, $"Exception while stopping listener: {e}");
            }
            finally
            {
                _listener = null;
            }
                
        }

        public event EventHandler<IDebuggerClient> OnClientConnected;
        public event EventHandler<ListenerErrorEventArgs> OnListenException;
    }
}