using ProtoBuf;
using System.Collections.Generic;

[ProtoContract]
public class NetworkSyncDataContainer {
  [ProtoMember(1, DataFormat = DataFormat.ZigZag)]
  public List<NetworkEntity> Entities = new List<NetworkEntity>(10);

  [ProtoMember(2, DataFormat = DataFormat.ZigZag)]
  public List<NetworkEntityData> AddedEntities = new List<NetworkEntityData>(10);

  [ProtoMember(3, DataFormat = DataFormat.ZigZag)]
  public List<EntityId> RemovedEntities = new List<EntityId>(10);
}