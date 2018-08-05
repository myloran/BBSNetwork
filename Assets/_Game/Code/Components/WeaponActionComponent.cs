using Unity.Entities;
using UnityEngine;

//[Serializable]
[Sync]
public struct WeaponAction : IComponentData {
    [SyncField]
    public boolean fire;
    [SyncField]
    public boolean reload;

    [SyncField(lerpDamp: 0)]
    [NetSyncSubMember("x"), NetSyncSubMember("y"), NetSyncSubMember("z")]
    public Vector3 shootOrigin;

    [SyncField(lerpDamp:0)]
    [NetSyncSubMember("x"), NetSyncSubMember("y"), NetSyncSubMember("z")]
    public Vector3 shootDir;

}

public class WeaponActionComponent : ComponentDataWrapper<WeaponAction> { }
