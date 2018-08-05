using Unity.Entities;
using UnityEngine;

//[Serializable]
[NetSync]
public struct WeaponAction : IComponentData {
    [FieldSync]
    public boolean fire;
    [FieldSync]
    public boolean reload;

    [FieldSync(lerpDamp: 0)]
    [NetSyncSubMember("x"), NetSyncSubMember("y"), NetSyncSubMember("z")]
    public Vector3 shootOrigin;

    [FieldSync(lerpDamp:0)]
    [NetSyncSubMember("x"), NetSyncSubMember("y"), NetSyncSubMember("z")]
    public Vector3 shootDir;

}

public class WeaponActionComponent : ComponentDataWrapper<WeaponAction> { }
