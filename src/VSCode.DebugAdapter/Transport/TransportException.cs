using System;

namespace VSCode.DebugAdapter.Transport
{
    public class TransportException : ApplicationException
    {
        public TransportException(string message) : base(message)
        {
        }
    }
}