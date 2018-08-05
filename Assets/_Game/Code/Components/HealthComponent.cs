using System;
using Unity.Entities;

[Sync]
[Serializable]
public struct Health : IComponentData {

    [SyncField]
    public int value;
}
public class HealthComponent : ComponentDataWrapper<Health> { }