using System;
using Unity.Entities;

[Sync]
[Serializable]
public struct Health : IComponentData {

    [FieldSync]
    public int value;
}
public class HealthComponent : ComponentDataWrapper<Health> { }