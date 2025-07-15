using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using OneScript.DebugProtocol.Abstractions;

namespace OneScript.DebugProtocol.TcpServer
{
    public class JsonDtoChannel : ICommunicationChannel
    {
        private readonly TcpClient _tcpClient;
        private readonly Stream _dataStream;
        
        private bool _enabled;

        public JsonDtoChannel(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _dataStream = tcpClient.GetStream();
        }

        public void Dispose()
        {
            _dataStream.Dispose();
            _tcpClient.Close();
            _enabled = false;
        }

        public void Write(object data)
        {
            if (!_enabled)
                throw new ObjectDisposedException(nameof(JsonDtoChannel));
            
            using var streamWriter = new StreamWriter(_dataStream, Encoding.UTF8, 1024, leaveOpen: true);
            using var writer = new JsonTextWriter(streamWriter);

            JsonSerializer.CreateDefault().Serialize(writer, data);
            writer.Flush();
        }

        public T Read<T>()
        {
            return (T)Read();
        }

        public object Read()
        {
            if (!_enabled)
                throw new ObjectDisposedException(nameof(JsonDtoChannel));
            
            using var streamReader = new StreamReader(_dataStream, Encoding.UTF8, false, 1024, leaveOpen: true);
            using var reader = new JsonTextReader(streamReader);

            return JsonSerializer.CreateDefault().Deserialize<RpcCall>(reader);
        }

        public bool Connected => _enabled && _tcpClient.Connected;
    }
}