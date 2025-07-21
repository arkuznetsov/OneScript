
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using OneScript.DebugProtocol.TcpServer;
using Xunit;

namespace VSCode.DebugAdapter.Tests
{
    public class ReconcileForOlderVersionTests
    {
        [Fact]
        public void CanReadReconcileViaBinaryFormatter()
        {
            using (var stream = new MemoryStream(FormatReconcileUtils.GetReconcileMagic()))
            {
                var reader = new BinaryFormatter();
                var value = reader.Deserialize(stream);
                value.Should().Be("1C1C1C");
            }
        }

        [Fact]
        public void ExchangeReconcileVersions()
        {
            using (var stream = new MemoryStream(FormatReconcileUtils.GetReconcileMagic()))
            {
                using(var response = new MemoryStream())
                {
                    if (FormatReconcileUtils.CheckReconcileRequest(stream))
                    {
                        FormatReconcileUtils.WriteReconcileResponse(response, 1, 1);
                    }

                    var data = response.ToArray();
                    Assert.True(FormatReconcileUtils.CheckReconcilePrefix(data));
                    Assert.Equal(FormatReconcileUtils.FORMAT_RECONCILE_RESPONSE_PREFIX.Length + sizeof(int), data.Length);
                    
                    var frmt = BitConverter.ToInt32(data, FormatReconcileUtils.FORMAT_RECONCILE_RESPONSE_PREFIX.Length);
                    var (transport, version) = FormatReconcileUtils.DecodeFormatMarker(frmt);
                    Assert.Equal(1, transport);
                    Assert.Equal(1, version);
                }
            }
        }
    }
}