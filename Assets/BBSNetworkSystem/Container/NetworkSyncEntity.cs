using ProtoBuf;

[ProtoContract]
public struct EntityId {
    [ProtoMember(1, IsPacked = true, DataFormat = DataFormat.ZigZag)]
    public int NetworkId;

    [ProtoMember(2, IsPacked = true, DataFormat = DataFormat.ZigZag)]
    public int ActorId;
}
