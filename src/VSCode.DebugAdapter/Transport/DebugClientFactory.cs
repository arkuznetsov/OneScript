/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;
using Serilog;

namespace VSCode.DebugAdapter.Transport
{
    public class DebugClientFactory
    {
        private readonly TcpClient _tcpClient;
        private readonly IDebugEventListener _eventsListener;

        private readonly ILogger Log = Serilog.Log.ForContext<DebugClientFactory>();

        public DebugClientFactory(TcpClient tcpClient, IDebugEventListener eventsListener)
        {
            _tcpClient = tcpClient;
            _eventsListener = eventsListener;
        }

        private TransportProtocols _transport = TransportProtocols.Invalid;
        private int _protocolVersion = ProtocolVersions.SafestVersion;

        public OneScriptDebuggerClient CreateDebugClient()
        {
            ReconcileDataFormat(_tcpClient);

            ICommunicationChannel commandsChannel;
            
            switch (_transport)
            {
                case TransportProtocols.Json:
                    commandsChannel = new JsonDtoChannel(_tcpClient);
                    break;
                case TransportProtocols.Binary:
                    commandsChannel = new BinaryChannel(_tcpClient);
                    break;
                default:
                    throw new InvalidOperationException($"Should not get here. [Transport protocol selection] {_transport}");
            };

            var client = new OneScriptDebuggerClient(commandsChannel, _eventsListener, _protocolVersion);
            client.Start();

            return client;
        }
        
        private void ReconcileDataFormat(TcpClient client)
        {
            Log.Verbose("Sending reconcile message");
            var stream = client.GetStream();
            stream.Write(FormatReconcileUtils.FORMAT_RECONCILE_MAGIC, 0, FormatReconcileUtils.FORMAT_RECONCILE_MAGIC.Length);

            var pollResult = client.Client
                .Poll(FormatReconcileUtils.FORMAT_RECONCILE_TIMEOUT.Milliseconds * 1000, SelectMode.SelectRead);
            
            if (pollResult)
            {
                Log.Verbose("Reconcile data available. Waiting for full data");
                var attempts = 0;
                const int WAIT_ATTEMPTS = 3;
                const int ATTEMPT_INTERVAL = 250;
                
                var requiredDataLength = FormatReconcileUtils.FORMAT_RECONCILE_RESPONSE_PREFIX.Length + sizeof(int);
                while (client.Client.Available < requiredDataLength && attempts < WAIT_ATTEMPTS)
                {
                    attempts++;
                    Log.Verbose("Attempt # {Attempt}", attempts);
                    Thread.Sleep(ATTEMPT_INTERVAL);
                }

                if (client.Client.Available >= requiredDataLength)
                {
                    Log.Verbose("We have data available. Reading reconcile response");
                    var dataBuffer = new byte[requiredDataLength];
                    using (var binaryReader = new BinaryReader(stream, Encoding.ASCII, true))
                    {
                        binaryReader.Read(dataBuffer, 0, FormatReconcileUtils.FORMAT_RECONCILE_RESPONSE_PREFIX.Length);

                        if (!FormatReconcileUtils.CheckReconcilePrefix(dataBuffer))
                        {
                            Log.Verbose("Received data is not reconcile message");
                            SelectSafestFormat();
                            return;
                        }


                        var formatMarker = binaryReader.ReadInt32();
                        var (transport, version) = FormatReconcileUtils.DecodeFormatMarker(formatMarker);
                        Log.Verbose("Received format marker {FormatVersion}. Transport {Transport}, Format {Format}",
                            formatMarker,
                            transport,
                            version);

                        if (transport != (int)TransportProtocols.Json)
                        {
                            throw new ApplicationException($"Transport protocol is out of range {transport}");
                        }

                        _transport = TransportProtocols.Json;
                        _protocolVersion = ProtocolVersions.Adjust(version);
                        Log.Verbose("Active protocol version {ProtocolVersion}", _protocolVersion);
                    }
                }
                else
                {
                    SelectSafestFormat();
                }
            }
            else
            {
                SelectSafestFormat();
            }
        }

        private void SelectSafestFormat()
        {
            Log.Verbose("Reconcilation failed, selecting safest format");
            _protocolVersion = ProtocolVersions.SafestVersion;
            _transport = TransportProtocols.Binary;
        }
    }
}
