using ProtoBuf;
using System.Collections.Generic;

[ProtoContract]
public class NetworkSyncDataContainer {
  [ProtoMember(1, DataFormat = DataFormat.ZigZag)]
  public List<NetworkEntityContainer> Entities = new List<NetworkEntityContainer>(10);

  [ProtoMember(2, DataFormat = DataFormat.ZigZag)]
  public List<NetworkEntityData> AddedEntities = new List<NetworkEntityData>(10);

  [ProtoMember(3, DataFormat = DataFormat.ZigZag)]
  public List<NetworkEntity> RemovedEntities = new List<NetworkEntity>(10);
}