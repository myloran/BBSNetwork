using ProtoBuf;
using System.Collections.Generic;

[ProtoContract]
public class NetworkEntityData {
    [ProtoMember(1, IsPacked = true, DataFormat = DataFormat.ZigZag)]
    public int InstanceId;

    [ProtoMember(2, DataFormat = DataFormat.ZigZag)]
    public EntityId NetworkSyncEntity;
    
    [ProtoMember(3, DataFormat = DataFormat.ZigZag)]
    public List<Components> ComponentData = new List<Components>(100);
}
