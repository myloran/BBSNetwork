using ProtoBuf;

[ProtoContract]
public struct ComponentField {
  [ProtoMember(1, IsPacked = true, DataFormat = DataFormat.ZigZag)]
  public int Id;

  [ProtoMember(2, IsPacked = true, DataFormat = DataFormat.ZigZag)]
  public int Value;
}