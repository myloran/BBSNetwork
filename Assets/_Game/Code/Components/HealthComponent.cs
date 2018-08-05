using System;
using Unity.Entities;

[NetSync]
[Serializable]
public struct Health : IComponentData {

    [NetSyncMember]
    public int value;
}
public class HealthComponent : ComponentDataWrapper<Health> { }