using ProtoBuf;
using System.Collections.Generic;

[ProtoContract]
public class NetworkEntityData {
    [ProtoMember(1, IsPacked = true, DataFormat = DataFormat.ZigZag)]
    public int InstanceId;

    [ProtoMember(2, DataFormat = DataFormat.ZigZag)]
    public EntityId Id;
    
    [ProtoMember(3, DataFormat = DataFormat.ZigZag)]
    public List<NetworkComponent> Components = new List<NetworkComponent>(100);
}
