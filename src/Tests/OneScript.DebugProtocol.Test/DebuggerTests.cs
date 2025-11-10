using System.IO;
using System.Threading;
using FluentAssertions;
using Moq;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;
using OneScript.DebugProtocol.Test.Tools;
using OneScript.DebugServices;
using ScriptEngine.Machine.Debugger;
using Xunit;

namespace OneScript.DebugProtocol.Test
{
    public class DebuggerTests
    {
        [Fact(DisplayName = "Отладчик ждет входящего клиента и блокируется на получении сессии")]
        public void TestBlockingOnIncomingClient()
        {
            var transport = new TestDebuggerTransport();
            var debugger = new DefaultDebugger(transport);
            debugger.WaitForSession = true;
            debugger.Start();

            var syncEvent = new AutoResetEvent(false);

            IDebugSession session = null;
            var waitingThread = new Thread(() =>
            {
                syncEvent.Set();
                session = debugger.GetSession();
                syncEvent.Set();
            })
            {
                IsBackground = true
            };
            
            waitingThread.Start();
            syncEvent.WaitOne(500).Should().BeTrue();
            Thread.Sleep(100);
            waitingThread.ThreadState.Should().HaveFlag(ThreadState.WaitSleepJoin);

            var testStream = new MemoryStream();
            testStream.Write(FormatReconcileUtils.GetReconcileMagic());
            testStream.Position = 0;
            
            var clientMock = new Mock<IDebuggerClient>();
            clientMock.Setup(s => s.GetDataStream())
                .Returns(testStream);
            
            transport.RaiseConnect(clientMock.Object);
            syncEvent.WaitOne(500).Should().BeTrue();

            Assert.NotNull(session);
            waitingThread.ThreadState.Should().Be(ThreadState.Stopped);
        }
        
        [Fact(DisplayName = "Отладчик не ждет входящего клиента и не блокируется на получении сессии")]
        public void TestNonBlockingOnIncomingClient()
        {
            var transport = new TestDebuggerTransport();
            var debugger = new DefaultDebugger(transport);
            debugger.WaitForSession = false;
            debugger.Start();

            var syncEvent = new AutoResetEvent(false);

            IDebugSession session = null;
            var waitingThread = new Thread(() =>
            {
                syncEvent.Set();
                session = debugger.GetSession();
                syncEvent.WaitOne();
            })
            {
                IsBackground = true
            };
            
            waitingThread.Start();
            syncEvent.WaitOne(500).Should().BeTrue();
            waitingThread.ThreadState.Should().HaveFlag(ThreadState.WaitSleepJoin);
            
            Assert.NotNull(session);
            syncEvent.Set();
        }
        
        //[Fact(DisplayName = "Отладчик отладчик не ждет входящего клиента, но ждет начала сессии")]
        public void TestNotBlockingAndWaitingForStart()
        {
            var transport = new TestDebuggerTransport();
            var debugger = new DefaultDebugger(transport);
            debugger.WaitForSession = false;
            debugger.Start();

            var syncEvent = new AutoResetEvent(false);
            transport.OnClientConnected += (sender, client) =>
            {
                syncEvent.Set();
            };
            
            var testStream = new MemoryStream();
            testStream.Write(FormatReconcileUtils.GetReconcileMagic());
            testStream.Position = 0;
            
            var clientMock = new Mock<IDebuggerClient>();
            clientMock.Setup(s => s.GetDataStream())
                .Returns(testStream);
            
            transport.RaiseConnect(clientMock.Object);
            syncEvent.WaitOne(500).Should().BeTrue();
            
            // Дождались соединения с сервером
            var session = debugger.GetSession();
            var waitingThread = new Thread(() =>
            {
                syncEvent.Set();
                session.WaitForStart();
            })
            {
                IsBackground = true
            };
            waitingThread.Start();
            syncEvent.WaitOne(500).Should().BeTrue();
            waitingThread.ThreadState.Should().HaveFlag(ThreadState.WaitSleepJoin);
            
            // Preparing start data
            testStream.SetLength(0);
            var rpcCall = RpcCall.Create(nameof(IDebuggerService), nameof(IDebuggerService.Execute), 0);
            
        }

    }
}