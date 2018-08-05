using Unity.Entities;
using UnityEngine;

[Sync]
public struct Position : IComponentData {
    [FieldSync(lerpDamp: 0.9f, jumpThreshold: 0)]
    [NetSyncSubMember("x")]
    [NetSyncSubMember("y")]
    [NetSyncSubMember("z")]
    public Vector3 Value;
}

public class PositionComponent : ComponentDataWrapper<Position> { }