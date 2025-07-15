using System.IO;
using System.Net.Sockets;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using OneScript.DebugProtocol.TcpServer;
using Xunit;

namespace OneScript.DebugProtocol.Test
{
    public class JsonChannelTest
    {
        [Fact]
        public void TestObjectReading()
        {
            var dataStream = new MemoryStream();
            var streamWriter = new StreamWriter(dataStream, Encoding.UTF8, 1024, leaveOpen: true);
            var rpcCall = RpcCall.Create("Test", "Hello World", 1, 2, new Breakpoint()
            {
                Line = 2, Condition = "true"
            });
            
            var jsonString = JsonConvert.SerializeObject(rpcCall);
            streamWriter.Write(jsonString);
            streamWriter.Close();
            dataStream.Position = 0;
            
            var channel = new JsonDtoChannel(dataStream);
            var actual = channel.Read<RpcCall>();
            actual.Should().BeEquivalentTo(rpcCall);
        }
        
        [Fact]
        public void TestObjectWriting()
        {
            var dataStream = new MemoryStream();
            var rpcCall = RpcCall.Create("Test", "Hello World", 1, 2, new Breakpoint()
            {
                Line = 2, Condition = "true"
            });
            
            var channel = new JsonDtoChannel(dataStream);
            channel.Write(rpcCall);
            dataStream.Position = 0;
            var actual = channel.Read<RpcCall>();
            actual.Should().BeEquivalentTo(rpcCall);
        }

        [Fact]
        public void TestReconcileMarker()
        {
            var encoded = FormatReconcileUtils.EncodeFormatMarker(2, 4);
            var (transport, version) = FormatReconcileUtils.DecodeFormatMarker(encoded);
            
            Assert.Equal(2, transport);
            Assert.Equal(4, version);
        }
    }
}