using ProtoBuf;
using System.Collections.Generic;

[ProtoContract]
public class SyncEntities {
  [ProtoMember(1, DataFormat = DataFormat.ZigZag)]
  public List<SyncEntity> Entities = new List<SyncEntity>(10);

  [ProtoMember(2, DataFormat = DataFormat.ZigZag)]
  public List<NetworkEntity> Added = new List<NetworkEntity>(10);

  [ProtoMember(3, DataFormat = DataFormat.ZigZag)]
  public List<EntityId> Removed = new List<EntityId>(10);
}