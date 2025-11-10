using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FluentAssertions;
using OneScript.DebugServices;
using Xunit;

namespace OneScript.DebugProtocol.Test
{
    public class TcpServerTest
    {
        [Fact]
        public void ServerCanStartAndStop()
        {
            var logMessages = new List<string>();
            var syncEvent = new AutoResetEvent(false);
            var server = new TcpDebugServer(0);
            server.OnLogEvent += (level, message) =>
            {
                if ((int)level >= (int)TcpDebugServer.LogLevel.Info)
                {
                    logMessages.Add(message);
                    syncEvent.Set();
                }
            };
            
            server.Listen();
            syncEvent.WaitOne(500).Should().BeTrue();
            server.Stop();
            syncEvent.WaitOne(500).Should().BeTrue();
            
            logMessages.Should()
                .HaveCount(2)
                .And.Contain("Starting listener thread")
                .And.Contain("Listener thread stopped");
        }

        [Fact]
        public void ServerCanAcceptClients()
        {
            var server = new TcpDebugServer(0);
            var syncEvent = new AutoResetEvent(false);
            server.OnClientConnected += (sender, client) =>
            {
                client.Should().NotBeNull();
                client.Connected.Should().BeTrue();
                syncEvent.Set();
            };
            
            server.Listen();
            var client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Loopback, server.ActualPort()));
            syncEvent.WaitOne(500).Should().BeTrue();

            client.Connected.Should().BeTrue();
            client.Dispose();
            server.Stop();
        }
    }
}