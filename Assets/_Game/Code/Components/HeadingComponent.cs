using Unity.Entities;
using UnityEngine;

[Sync]
public struct Heading : IComponentData {
    [SyncField(lerpDamp:0.9f)]
    [NetSyncSubMember("x")]
    [NetSyncSubMember("z")]
    public Vector3 Value;

    public Heading(Vector3 heading) {
        Value = heading;
    }
}

public class HeadingComponent : ComponentDataWrapper<Heading> { }