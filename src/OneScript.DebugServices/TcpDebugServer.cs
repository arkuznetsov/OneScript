/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Net.Sockets;
using OneScript.DebugProtocol.TcpServer;
using OneScript.DebugServices.Internal;
using ScriptEngine.Machine;

namespace OneScript.DebugServices
{
    public class TcpDebugServer
    {
        private readonly int _port;

        public TcpDebugServer(int port)
        {
            _port = port;
        }

        public IDebugController CreateDebugController()
        {
            // Use reconnectable controller to support multiple attach/detach cycles
            return new ReconnectableDebugController(_port);
        }
    }
}