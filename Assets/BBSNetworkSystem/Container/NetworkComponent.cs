using ProtoBuf;
using System.Collections.Generic;

[ProtoContract]
public class NetworkComponent {
  [ProtoMember(1, IsPacked = true, DataFormat = DataFormat.ZigZag)]
  public int TypeId;

  [ProtoMember(2, DataFormat = DataFormat.ZigZag)]
  public List<ComponentField> Fields = new List<ComponentField>();
}