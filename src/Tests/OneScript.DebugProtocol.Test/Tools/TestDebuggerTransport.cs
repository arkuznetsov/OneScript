using System;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugServices;

namespace OneScript.DebugProtocol.Test.Tools
{
    public class TestDebuggerTransport : IDebugServer
    {
        public void Listen()
        {
        }

        public void Stop()
        {
        }

        public void RaiseConnect(IDebuggerClient client)
        {
            OnClientConnected?.Invoke(this, client);
        }

        public event EventHandler<IDebuggerClient> OnClientConnected;
        public event EventHandler<ListenerErrorEventArgs> OnListenException;
    }
}