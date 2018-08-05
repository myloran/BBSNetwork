using System;
/// <summary>
/// Requires signature of: 
/// static Entity (EntityManager)
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SpawnAttribute : Attribute {
    public readonly int InstanceId;
    public SpawnAttribute(int instanceId) {
        InstanceId = instanceId;
    }
}
