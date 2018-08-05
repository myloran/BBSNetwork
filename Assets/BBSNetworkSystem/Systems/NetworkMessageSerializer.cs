using ProtoBuf;
using System;
using System.IO;

public class NetworkMessageSerializer<T> : IDisposable {
  MemoryStream stream = new MemoryStream();

  public T Deserialize(byte[] data) {
    //memoryStream.Seek(0, SeekOrigin.Begin);
    //memoryStream.Write(data, 0, data.Length);
    //memoryStream.Position = 0;
    using (var stream = new MemoryStream(data)) {
      return Serializer.Deserialize<T>(stream);
    }
  }

  public void Dispose() {
    stream.Dispose();
  }

  public byte[] Serialize(T data) {
    //memoryStream.Seek(0, SeekOrigin.Begin);
    //memoryStream.Position = 0;
    using (var stream = new MemoryStream()) {
      Serializer.Serialize(stream, data);
      return stream.ToArray();
    }
  }
}