using Unity.Entities;

//[Serializable]
[Sync]
public struct DeathComponent : IComponentData {
    [FieldSync(initOnly: true)]
    public float timer;
}

//public class DeathComponent  : ComponentDataWrapper<DeathComponent> { }
