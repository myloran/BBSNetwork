using Unity.Entities;

//[Serializable]
[NetSync]
public struct DeathComponent : IComponentData {
    [NetSyncMember(initOnly: true)]
    public float timer;
}

//public class DeathComponent  : ComponentDataWrapper<DeathComponent> { }
