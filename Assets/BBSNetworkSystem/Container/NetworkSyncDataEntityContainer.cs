using ProtoBuf;
using System.Collections.Generic;

[ProtoContract]
public class NetworkEntity {
  [ProtoMember(1, DataFormat = DataFormat.ZigZag)]
  public EntityId Id;

  [ProtoMember(2, DataFormat = DataFormat.ZigZag)]
  public List<Components> AddedComponents = new List<Components>(10);

  [ProtoMember(3, DataFormat = DataFormat.ZigZag)]
  public List<int> RemovedComponents = new List<int>(10);

  [ProtoMember(4, DataFormat = DataFormat.ZigZag)]
  public List<Components> Components = new List<Components>(100);
}