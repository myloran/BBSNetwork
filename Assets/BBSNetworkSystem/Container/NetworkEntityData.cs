using ProtoBuf;
using System.Collections.Generic;

[ProtoContract]
public class NetworkEntityData {
    [ProtoMember(1, IsPacked = true, DataFormat = DataFormat.ZigZag)]
    public int InstanceId;

    [ProtoMember(2, DataFormat = DataFormat.ZigZag)]
    public NetworkEntity NetworkSyncEntity;
    
    [ProtoMember(3, DataFormat = DataFormat.ZigZag)]
    public List<ComponentDataContainer> ComponentData = new List<ComponentDataContainer>(100);
}
