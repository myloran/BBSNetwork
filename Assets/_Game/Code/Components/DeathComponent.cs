using Unity.Entities;

//[Serializable]
[NetSync]
public struct DeathComponent : IComponentData {
    [FieldSync(initOnly: true)]
    public float timer;
}

//public class DeathComponent  : ComponentDataWrapper<DeathComponent> { }
